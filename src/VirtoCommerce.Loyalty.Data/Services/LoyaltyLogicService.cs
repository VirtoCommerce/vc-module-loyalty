using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Models.Rewards;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DistributedLock;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyLogicService : ILoyaltyLogicService, IProductLoyaltyProgramService
{
    private readonly ILoyaltyProgramSearchService _loyaltyProgramSearchService;
    private readonly ILoyaltyProgramOperationLogService _loyaltyProgramOperationLogService;
    private readonly ILoyaltyProgramOperationLogSearchService _loyaltyProgramOperationLogSearchService;
    private readonly IMemberResolver _memberResolver;
    private readonly ICustomerOrderService _customerOrderService;
    private readonly ICustomerOrderSearchService _customerOrderSearchService;
    private readonly IDistributedLockService _distributedLockService;

    public LoyaltyLogicService(
        ILoyaltyProgramSearchService loyaltyProgramSearchService,
        ILoyaltyProgramOperationLogService loyaltyProgramOperationLogService,
        ILoyaltyProgramOperationLogSearchService loyaltyProgramOperationLogSearchService,
        IMemberResolver memberResolver,
        ICustomerOrderService customerOrderService,
        ICustomerOrderSearchService customerOrderSearchService,
        IDistributedLockService distributedLockService)
    {
        _loyaltyProgramSearchService = loyaltyProgramSearchService;
        _loyaltyProgramOperationLogService = loyaltyProgramOperationLogService;
        _loyaltyProgramOperationLogSearchService = loyaltyProgramOperationLogSearchService;
        _memberResolver = memberResolver;
        _customerOrderService = customerOrderService;
        _customerOrderSearchService = customerOrderSearchService;
        _distributedLockService = distributedLockService;
    }

    public async IAsyncEnumerable<LoyaltyProgram> GetActiveLoyaltyProgramsAsync(string[] storeIds, string programType)
    {
        var criteria = AbstractTypeFactory<LoyaltyProgramSearchCriteria>.TryCreateInstance();
        criteria.StoreIds = storeIds;
        criteria.OnlyActive = true;
        criteria.ProgramType = programType;
        criteria.Sort = "priority:desc";

        await foreach (var searchResult in _loyaltyProgramSearchService.SearchBatchesNoCloneAsync(criteria))
        {
            foreach (var loyaltyProgram in searchResult.Results)
            {
                yield return loyaltyProgram;
            }
        }
    }

    public async Task<decimal> GetUserBalanceAsync(string userId)
    {
        var operationLog = await GetLastLoyaltyOperationLogByUser(userId);
        return operationLog?.Balance ?? 0;
    }

    public async Task<LoyaltyBalanceResult> GetLoyaltyBalanceAsync(LoyaltyBalanceRequest request)
    {
        // if UserId is not provided get user from order, if order is not provided return 0
        var result = new LoyaltyBalanceResult();
        var order = request.CustomerOrder;
        var userId = request.UserId;

        if (order == null && !request.OrderId.IsNullOrEmpty())
        {
            order = await _customerOrderService.GetNoCloneAsync(request.OrderId, CustomerOrderResponseGroup.WithPrices.ToString());
        }

        if (userId.IsNullOrEmpty() && order != null)
        {
            userId = order.CustomerId;
        }

        if (userId.IsNullOrEmpty())
        {
            return result;
        }

        result.CurrentBalance = result.ResultBalance = await GetUserBalanceAsync(userId);

        if (order != null)
        {
            result.ResultBalance = result.CurrentBalance - order.Total;
        }

        return result;
    }

    public async Task<bool> IsObjectProcessedAsync(string objectType, string objectId, string operationType)
    {
        var criteria = AbstractTypeFactory<LoyaltyProgramOperationLogSearchCriteria>.TryCreateInstance();
        criteria.ObjectType = objectType;
        criteria.ObjectId = objectId;
        criteria.OperationType = operationType;
        criteria.Take = 0;

        var searchResult = await _loyaltyProgramOperationLogSearchService.SearchNoCloneAsync(criteria);

        return searchResult.TotalCount > 0;
    }

    public async Task<List<string>> FindProcessedObjectIdsAsync(string objectType, string[] objectIds)
    {
        var result = new List<string>();

        // rewrite to batch processing
        foreach (var objectId in objectIds)
        {
            // registration / award path tracks "Earned" operations only
            if (await IsObjectProcessedAsync(objectType, objectId, ModuleConstants.LoyaltyPrograms.EarnedOperationType))
            {
                result.Add(objectId);
            }
        }

        return result;
    }

    public async Task PopulateLoyaltyProgramEvaluationContextAsync(LoyaltyProgramEvaluationContext context)
    {
        if (context.ContextObjectType == nameof(CustomerOrder) && !context.OrderId.IsNullOrEmpty())
        {
            await PopulateLoyaltyProgramEvaluationContextByOrderAsync(context.OrderId, context);
        }

        if (!context.UserId.IsNullOrEmpty())
        {
            context.UserGroups = await GetUserGroups(context.UserId);
        }
    }

    private async Task PopulateLoyaltyProgramEvaluationContextByOrderAsync(string orderId, LoyaltyProgramEvaluationContext context)
    {
        var order = await _customerOrderService.GetNoCloneAsync(orderId, CustomerOrderResponseGroup.WithPrices.ToString());

        if (order == null)
        {
            return;
        }

        context.Language = order.LanguageCode;
        context.CurrencyCode = order.Currency;
        context.StoreId = order.StoreId;
        context.UserId = order.CustomerId;

        context.OrderStatus = order.Status;
        context.OrderTotal = order.Total;

        context.IsRecurringOrder = order.SubscriptionId != null;
        context.IsFirstOrder = await EvaluateIsFirstOrder(context);
    }

    private async Task<bool> EvaluateIsFirstOrder(LoyaltyProgramEvaluationContext context)
    {
        var ordersCount = await _customerOrderSearchService.SearchNoCloneAsync(new CustomerOrderSearchCriteria
        {
            CustomerId = context.UserId,
            StoreIds = [context.StoreId],
            WithPrototypes = false,
            Take = 0,
            Skip = 0,
        });

        return ordersCount.TotalCount == 1;
    }

    public async Task<LoyaltyAmountResult> EvaluateLoyaltyProgramsAsync(LoyaltyProgramEvaluationContext loyaltyContext)
    {
        var allRewards = new List<LoyaltyReward>();

        await PopulateLoyaltyProgramEvaluationContextAsync(loyaltyContext);

        var maxPriority = 0;

        await foreach (var loyaltyProgram in GetActiveLoyaltyProgramsAsync([loyaltyContext.StoreId], loyaltyContext.ProgramType))
        {
            var isSatisfied = loyaltyProgram.DynamicExpression.IsSatisfiedBy(loyaltyContext);
            if (!isSatisfied)
            {
                continue;
            }

            var programRewards = loyaltyProgram.DynamicExpression.GetLoyaltyRewards();

            foreach (var reward in programRewards)
            {
                reward.LoyaltyProgram = loyaltyProgram;
            }

            allRewards.AddRange(programRewards);

            if (loyaltyProgram.Priority > maxPriority)
            {
                maxPriority = loyaltyProgram.Priority;
            }
        }

        if (allRewards.Count == 0)
        {
            return null;
        }

        var maxPriotiryLoyaltyProgramIds = allRewards
            .Where(x => x.LoyaltyProgram.Priority == maxPriority)
            .Select(x => x.LoyaltyProgram.Id)
            .Distinct()
            .ToArray();

        var summedRewardsByProgramId = allRewards
            .GroupBy(x => x.LoyaltyProgram.Id)
            .Select(x => new LoyaltyAmountResult
            {
                LoyaltyProgramId = x.Key,
                Amount = x.Sum(x => x.GetActualRewardAmount(loyaltyContext.OrderTotal))
            })
            .ToArray();

        var maxReward = summedRewardsByProgramId
            .Where(x => maxPriotiryLoyaltyProgramIds.Contains(x.LoyaltyProgramId))
            .OrderByDescending(x => x.Amount)
            .FirstOrDefault();

        if (maxReward == null)
        {
            return null;
        }

        maxReward.OperationType = ModuleConstants.LoyaltyPrograms.EarnedOperationType; // Assuming "Earned" is the operation type for rewards
        return maxReward;
    }

    public Task<bool> LogLoyaltyProgramOperationAsync(LoyaltyProgramEvaluationContext loyaltyContext, LoyaltyAmountResult loyaltyResult)
    {
        // Serialize the per-user balance read-modify-write across all operation sources
        // (earn, mixed-cart redeem, payment-method redeem) so concurrent operations for the
        // same user cannot read a stale balance and overwrite each other's running total.
        return _distributedLockService.ExecuteAsync($"loyalty-balance:{loyaltyContext.UserId}",
            () => LogLoyaltyProgramOperationInternalAsync(loyaltyContext, loyaltyResult),
            lockTimeout: TimeSpan.FromSeconds(30),
            tryLockTimeout: TimeSpan.FromSeconds(30),
            retryInterval: TimeSpan.FromMilliseconds(200));
    }

    private async Task<bool> LogLoyaltyProgramOperationInternalAsync(LoyaltyProgramEvaluationContext loyaltyContext, LoyaltyAmountResult loyaltyResult)
    {
        // dedup against the specific operation type being logged, so earn and redeem
        // for the same object do not block each other and repeats are still skipped
        if (await IsObjectProcessedAsync(loyaltyContext.ContextObjectType, loyaltyContext.ContextObjectId, loyaltyResult.OperationType))
        {
            return false;
        }

        var operationLog = AbstractTypeFactory<LoyaltyProgramOperationLog>.TryCreateInstance();
        operationLog.OperationType = loyaltyResult.OperationType;
        operationLog.ObjectType = loyaltyContext.ContextObjectType;
        operationLog.ObjectId = loyaltyContext.ContextObjectId;
        operationLog.UserId = loyaltyContext.UserId;
        operationLog.LoyaltyProgramId = loyaltyResult.LoyaltyProgramId;
        operationLog.Amount = loyaltyResult.Amount;

        var balance = await GetUserBalanceAsync(loyaltyContext.UserId);
        operationLog.Balance = operationLog.OperationType switch
        {
            ModuleConstants.LoyaltyPrograms.EarnedOperationType => balance + loyaltyResult.Amount,
            _ => balance - loyaltyResult.Amount,
        };

        await _loyaltyProgramOperationLogService.SaveChangesAsync([operationLog]);

        return true;
    }

    public async Task<LoyaltyProgram> GetTopLoyaltyProgramAsync(LoyaltyProgramEvaluationContext loyaltyContext)
    {
        await PopulateLoyaltyProgramEvaluationContextAsync(loyaltyContext);

        await foreach (var loyaltyProgram in GetActiveLoyaltyProgramsAsync([loyaltyContext.StoreId], loyaltyContext.ProgramType))
        {
            var isSatisfied = loyaltyProgram.DynamicExpression.IsSatisfiedBy(loyaltyContext);
            if (isSatisfied)
            {
                return loyaltyProgram;
            }
        }

        return null;
    }

    private async Task<LoyaltyProgramOperationLog> GetLastLoyaltyOperationLogByUser(string userId)
    {
        var criteria = AbstractTypeFactory<LoyaltyProgramOperationLogSearchCriteria>.TryCreateInstance();
        criteria.UserId = userId;
        criteria.Take = 1;
        criteria.Sort = "CreatedDate:desc"; // Assuming we want the most recent operation log for balance calculation

        var searchResult = await _loyaltyProgramOperationLogSearchService.SearchNoCloneAsync(criteria);

        return searchResult.Results?.FirstOrDefault();
    }

    private async Task<string[]> GetUserGroups(string userId)
    {
        var member = await _memberResolver.ResolveMemberByIdAsync(userId);
        if (member == null)
        {
            return null;
        }

        return member.Groups.ToArray();
    }
}

