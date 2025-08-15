using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.DistributedLock;

namespace VirtoCommerce.Loyalty.Data.Handlers;

public class LoyaltyProgramOrderHandler : IEventHandler<OrderChangedEvent>
{
    private readonly ILoyaltyLogicService _loyaltyLogicService;
    private readonly IInternalDistributedLockService _distributedLockProvider;

    public LoyaltyProgramOrderHandler(
        ILoyaltyLogicService loyaltyLogicService,
        IInternalDistributedLockService distributedLockProvider)
    {
        _loyaltyLogicService = loyaltyLogicService;
        _distributedLockProvider = distributedLockProvider;
    }

    public virtual Task Handle(OrderChangedEvent message)
    {
        var loyaltyContexts = message.ChangedEntries
            .Where(x => (x.EntryState == EntryState.Added || x.EntryState == EntryState.Modified) && x.NewEntry.IsPrototype == false)
            .OrderBy(x => x.NewEntry.ModifiedDate)
            .Select(x => CreateLoyaltyContextByOrder(x.NewEntry))
            .ToList();

        if (loyaltyContexts.Count > 0)
        {
            BackgroundJob.Enqueue(() => ProcessOrderAwardsAsync(loyaltyContexts));
        }

        return Task.CompletedTask;
    }

    [DisableConcurrentExecution(10)]
    public async Task ProcessOrderAwardsAsync(IList<LoyaltyProgramEvaluationContext> loyaltyContexts)
    {
        // disable context if order already processed by loyalty logic
        var orderIds = loyaltyContexts
            .Select(x => x.OrderId)
            .Distinct()
            .ToArray();

        var processedOrderIds = await _loyaltyLogicService.FindProcessedOrderIdsAsync(orderIds);

        loyaltyContexts = loyaltyContexts
            .Where(x => !processedOrderIds.Contains(x.OrderId))
            .ToList();

        if (loyaltyContexts.Count == 0)
        {
            return;
        }

        foreach (var context in loyaltyContexts)
        {
            // Evaluate loyalty programs for the context
            var loyaltyReward = await _loyaltyLogicService.EvaluateLoyaltyProgramsAsync(context);

            if (loyaltyReward == null)
            {
                continue;
            }

            _distributedLockProvider.ExecuteSynchronized($"loyalty-usage:{context.OrderId}", async (x) =>
            {
                if (x == DistributedLockCondition.Delayed)
                {
                    // If the lock is delayed, we can skip processing this order
                    return;
                }

                await _loyaltyLogicService.LogLoyaltyUsageAsync(context, loyaltyReward);
            });
        }
    }

    private static LoyaltyProgramEvaluationContext CreateLoyaltyContextByOrder(CustomerOrder order)
    {
        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.OrderId = order.Id;
        return context;
    }

    private static LoyaltyProgramEvaluationContext CreateLoyaltyContextForRegistration(CustomerOrder order)
    {
        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.IsRegistration = true;
        return context;
    }
}
