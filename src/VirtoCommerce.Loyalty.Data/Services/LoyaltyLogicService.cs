using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Models.Rewards;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
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
        var usage = await GetLastLoyaltyPrgoramUsageByUser(userId);
        return usage?.Balance ?? 0;
    }

    public async Task<bool> IsOrderProcessedAsync(string orderId)
    {
        var criteria = AbstractTypeFactory<LoyaltyProgramUsageSearchCriteria>.TryCreateInstance();
        criteria.OrderId = orderId;
        criteria.UsageType = ModuleConstants.LoyaltyPrograms.AwardedUsageType; // Assuming "Awarded" is the usage type for processed orders
        criteria.Take = 0;

        var searchResult = await _loyaltyProgramUsageSearchService.SearchNoCloneAsync(criteria);

        return searchResult.TotalCount > 0;
    }

    public async Task<List<string>> FindProcessedOrderIdsAsync(string[] orderIds)
    {
        var result = new List<string>();

        // todo: rewrite to batch processing
        foreach (var orderId in orderIds)
        {
            if (await IsOrderProcessedAsync(orderId))
            {
                result.Add(orderId);
            }
        }

        return result;
    }

    public async Task PopulateLoyaltyProgramEvaluationContext(LoyaltyProgramEvaluationContext context)
    {
        var order = await _customerOrderService.GetNoCloneAsync(context.OrderId, CustomerOrderResponseGroup.Default.ToString());

        if (order == null)
        {
            return;
        }

        context.Language = order.LanguageCode;
        context.CurrencyCode = order.Currency;
        context.StoreId = order.StoreId;
        context.UserId = order.CustomerId;
        context.UserGroups = await GetUserGroups(context.UserId);

        context.OrderStatus = order.Status;
        context.OrderTotal = order.Total;

        context.IsRecurringOrder = order.SubscriptionId != null; // ???
        //context.IsFirstOrder = 
        //context.IsRegistration =
    }

    public async Task<LoyaltyProgramsEvaluationResult> EvaluateLoyaltyProgramsAsync(LoyaltyProgramEvaluationContext loyaltyContext)
    {
        var allRewards = new List<LoyaltyReward>();

        await PopulateLoyaltyProgramEvaluationContext(loyaltyContext);

        await foreach (var loyaltyProgram in GetActiveLoyaltyProgramsAsync([loyaltyContext.StoreId]))
        {
            var isSatisfied = loyaltyProgram.DynamicExpression.IsSatisfiedBy(loyaltyContext);
            if (isSatisfied)
            {
                var programRewards = loyaltyProgram.DynamicExpression.GetLoyaltyRewards();

                foreach (var reward in programRewards)
                {
                    reward.LoyaltyProgramId = loyaltyProgram.Id;
                }

                allRewards.AddRange(programRewards);
            }
        }

        var bestLoyaltyReward = allRewards
            .Select(x => new LoyaltyProgramsEvaluationResult { Reward = x, ActualRewardAmount = x.GetActualRewardAmount(loyaltyContext.OrderTotal) })
            .OrderByDescending(x => x.ActualRewardAmount)
            .FirstOrDefault();

        return bestLoyaltyReward;
    }

    public async Task LogLoyaltyUsageAsync(LoyaltyProgramEvaluationContext loyaltyContext, LoyaltyProgramsEvaluationResult loyaltyResult)
    {
        if (await IsOrderProcessedAsync(loyaltyContext.OrderId))
        {
            return;
        }

        var balance = await GetUserBalanceAsync(loyaltyContext.UserId);

        var usage = AbstractTypeFactory<LoyaltyProgramUsage>.TryCreateInstance();
        usage.UserId = loyaltyContext.UserId;
        usage.OrderId = loyaltyContext.OrderId;
        usage.LoyaltyProgramId = loyaltyResult.Reward.LoyaltyProgramId;
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

