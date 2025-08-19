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

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyLogicService : ILoyaltyLogicService
{
    private readonly ILoyaltyProgramService _loyaltyProgramService;
    private readonly ILoyaltyProgramSearchService _loyaltyProgramSearchService;
    private readonly ILoyaltyProgramUsageService _loyaltyProgramUsageService;
    private readonly ILoyaltyProgramUsageSearchService _loyaltyProgramUsageSearchService;
    private readonly IMemberResolver _memberResolver;
    private readonly ICustomerOrderService _customerOrderService;
    private readonly ICustomerOrderSearchService _customerOrderSearchService;

    public LoyaltyLogicService(
        ILoyaltyProgramService loyaltyProgramService,
        ILoyaltyProgramSearchService loyaltyProgramSearchService,
        ILoyaltyProgramUsageService loyaltyProgramUsageService,
        ILoyaltyProgramUsageSearchService loyaltyProgramUsageSearchService,
        IMemberResolver memberResolver,
        ICustomerOrderService customerOrderService,
        ICustomerOrderSearchService customerOrderSearchService)
    {
        _loyaltyProgramService = loyaltyProgramService;
        _loyaltyProgramSearchService = loyaltyProgramSearchService;
        _loyaltyProgramUsageService = loyaltyProgramUsageService;
        _loyaltyProgramUsageSearchService = loyaltyProgramUsageSearchService;
        _memberResolver = memberResolver;
        _customerOrderService = customerOrderService;
        _customerOrderSearchService = customerOrderSearchService;
    }

    public async IAsyncEnumerable<LoyaltyProgram> GetActiveLoyaltyProgramsAsync(string[] storeIds)
    {
        var criteria = AbstractTypeFactory<LoyaltyProgramSearchCriteria>.TryCreateInstance();
        criteria.StoreIds = storeIds;
        criteria.OnlyActive = true;
        criteria.Sort = "priority:desc";

        await foreach (var searchResult in _loyaltyProgramSearchService.SearchBatchesNoCloneAsync(criteria))
        {
            foreach (var loyaltyProgram in searchResult.Results)
            {
                yield return loyaltyProgram;
            }
        }
    }

    public async Task<LoyaltyProgram> GetActiveLoyaltyProgramAsync(string storeId)
    {
        var criteria = AbstractTypeFactory<LoyaltyProgramSearchCriteria>.TryCreateInstance();
        criteria.StoreId = storeId;
        criteria.OnlyActive = true;
        criteria.Sort = "priority:desc";
        criteria.Take = 1;

        var searchResult = await _loyaltyProgramSearchService.SearchNoCloneAsync(criteria);
        return searchResult.Results?.FirstOrDefault();
    }

    public async Task<decimal> GetUserBalanceAsync(string userId)
    {
        var usage = await GetLastLoyaltyPrgoramUsageByUser(userId);
        return usage?.Balance ?? 0;
    }

    public async Task<bool> IsObjectProcessedAsync(string objectType, string objectId)
    {
        var criteria = AbstractTypeFactory<LoyaltyProgramUsageSearchCriteria>.TryCreateInstance();
        criteria.ObjectType = objectType;
        criteria.ObjectId = objectId;
        criteria.UsageType = ModuleConstants.LoyaltyPrograms.AwardedUsageType; // Assuming "Awarded" is the usage type for processed orders
        criteria.Take = 0;

        var searchResult = await _loyaltyProgramUsageSearchService.SearchNoCloneAsync(criteria);

        return searchResult.TotalCount > 0;
    }

    public async Task<List<string>> FindProcessedObjectIdsAsync(string objectType, string[] objectIds)
    {
        var result = new List<string>();

        // rewrite to batch processing
        foreach (var objectId in objectIds)
        {
            if (await IsObjectProcessedAsync(objectType, objectId))
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

    public async Task<LoyaltyProgramsEvaluationResult> EvaluateLoyaltyProgramsAsync(LoyaltyProgramEvaluationContext loyaltyContext)
    {
        var allRewards = new List<LoyaltyReward>();

        await PopulateLoyaltyProgramEvaluationContextAsync(loyaltyContext);

        var maxPriority = 0;

        await foreach (var loyaltyProgram in GetActiveLoyaltyProgramsAsync([loyaltyContext.StoreId]))
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
            .Select(x => new LoyaltyProgramsEvaluationResult
            {
                LoyaltyProgramId = x.Key,
                ActualRewardAmount = x.Sum(x => x.GetActualRewardAmount(loyaltyContext.OrderTotal))
            })
            .ToArray();

        var maxReward = summedRewardsByProgramId
            .Where(x => maxPriotiryLoyaltyProgramIds.Contains(x.LoyaltyProgramId))
            .OrderByDescending(x => x.ActualRewardAmount)
            .FirstOrDefault();

        return maxReward;
    }

    public async Task LogLoyaltyUsageAsync(LoyaltyProgramEvaluationContext loyaltyContext, LoyaltyProgramsEvaluationResult loyaltyResult)
    {
        if (await IsObjectProcessedAsync(loyaltyContext.ContextObjectType, loyaltyContext.ContextObjectId))
        {
            return;
        }

        var balance = await GetUserBalanceAsync(loyaltyContext.UserId);

        var usage = AbstractTypeFactory<LoyaltyProgramUsage>.TryCreateInstance();
        usage.ObjectType = loyaltyContext.ContextObjectType;
        usage.ObjectId = loyaltyContext.ContextObjectId;
        usage.UserId = loyaltyContext.UserId;
        usage.LoyaltyProgramId = loyaltyResult.LoyaltyProgramId;
        usage.UsageType = ModuleConstants.LoyaltyPrograms.AwardedUsageType; // Assuming "Awarded" is the usage type for rewards
        usage.Points = loyaltyResult.ActualRewardAmount;
        usage.Balance = balance += loyaltyResult.ActualRewardAmount;

        await _loyaltyProgramUsageService.SaveChangesAsync([usage]);
    }

    private async Task<LoyaltyProgramUsage> GetLastLoyaltyPrgoramUsageByUser(string userId)
    {
        var criteria = AbstractTypeFactory<LoyaltyProgramUsageSearchCriteria>.TryCreateInstance();
        criteria.UserId = userId;
        criteria.Take = 1;
        criteria.Sort = "CreatedDate:desc"; // Assuming we want the most recent usage for balance calculation

        var searchResult = await _loyaltyProgramUsageSearchService.SearchNoCloneAsync(criteria);

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

