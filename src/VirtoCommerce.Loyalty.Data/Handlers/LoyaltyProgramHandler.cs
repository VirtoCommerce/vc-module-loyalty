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
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.Platform.DistributedLock;

namespace VirtoCommerce.Loyalty.Data.Handlers;

public class LoyaltyProgramHandler : IEventHandler<OrderChangedEvent>, IEventHandler<UserChangedEvent>
{
    private readonly ILoyaltyLogicService _loyaltyLogicService;
    private readonly IInternalDistributedLockService _distributedLockProvider;

    public LoyaltyProgramHandler(
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
            BackgroundJob.Enqueue(() => ProcessAwardsAsync(loyaltyContexts, nameof(CustomerOrder)));
        }

        return Task.CompletedTask;
    }

    public virtual Task Handle(UserChangedEvent message)
    {
        var loyaltyContexts = message.ChangedEntries
            .Where(x => (x.EntryState == EntryState.Added))
            .Select(x => CreateLoyaltyContextByUser(x.NewEntry))
            .ToList();

        if (loyaltyContexts.Count > 0)
        {
            BackgroundJob.Enqueue(() => ProcessAwardsAsync(loyaltyContexts, nameof(ApplicationUser)));
        }

        return Task.CompletedTask;
    }

    [DisableConcurrentExecution(10)]
    public async Task ProcessAwardsAsync(IList<LoyaltyProgramEvaluationContext> loyaltyContexts, string objectType)
    {
        // disable context if object already processed by loyalty logic
        var objectIds = loyaltyContexts
            .Select(x => x.ContextObjectId)
            .Distinct()
            .ToArray();

        var processedObjectIds = await _loyaltyLogicService.FindProcessedObjectIdsAsync(objectType, objectIds);

        loyaltyContexts = loyaltyContexts
            .Where(x => !processedObjectIds.Contains(x.ContextObjectId))
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

            _distributedLockProvider.ExecuteSynchronized($"loyalty-usage:{context.ContextObjectType}:{context.ContextObjectId}", async (x) =>
            {
                if (x == DistributedLockCondition.Delayed)
                {
                    // If the lock is delayed, we can skip processing this object
                    return;
                }

                await _loyaltyLogicService.LogLoyaltyUsageAsync(context, loyaltyReward);
            });
        }
    }

    private static LoyaltyProgramEvaluationContext CreateLoyaltyContextByOrder(CustomerOrder order)
    {
        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.ContextObjectType = nameof(CustomerOrder);
        context.OrderId = order.Id;
        return context;
    }

    private static LoyaltyProgramEvaluationContext CreateLoyaltyContextByUser(ApplicationUser user)
    {
        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.ContextObjectType = nameof(ApplicationUser);
        context.UserId = user.Id;
        context.StoreId = user.StoreId;
        context.IsRegistration = true;
        return context;
    }
}
