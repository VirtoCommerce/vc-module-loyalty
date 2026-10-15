using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.BackgroundJobs;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.Loyalty.Data.Handlers;

public class LoyaltyMissionHandler(
    ILoyaltyMissionLogicService missionLogicService,
    IStoreService storeService,
    ICustomerOrderService customerOrderService)
    : IEventHandler<OrderChangedEvent>
{
    public virtual Task Handle(OrderChangedEvent message)
    {
        var orderIds = message.ChangedEntries
            .Where(x => x.EntryState == EntryState.Added)
            .Select(x => x.NewEntry)
            .Where(x => !x.IsPrototype)
            .OrderBy(x => x.ModifiedDate)
            .Select(x => x.Id)
            .Distinct()
            .ToArray();

        if (orderIds.Length > 0)
        {
            var payload = AbstractTypeFactory<ProcessLoyaltyMissionsJobPayload>.TryCreateInstance();
            payload.OrderIds = orderIds;

            // The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from
            // the root provider and holds it for the process lifetime, so it must not capture a Scoped dependency.
            return BackgroundJob.Enqueue<ProcessLoyaltyMissionsJobHandler>(payload);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Processes loyalty missions for newly placed orders. Runs from <see cref="ProcessLoyaltyMissionsJobHandler"/>, which
    /// takes the distributed lock that replaced Hangfire's [DisableConcurrentExecution]. Its signature is unchanged on
    /// purpose: jobs queued by an earlier version reference this method by name.
    /// </summary>
    public async Task ProcessMissionsAsync(string[] orderIds)
    {
        var orders = await customerOrderService.GetNoCloneAsync(orderIds);
        if (orders.Count == 0)
        {
            return;
        }

        var storeIds = orders.Select(x => x.StoreId).Distinct().ToArray();
        var stores = await storeService.GetNoCloneAsync(storeIds);

        foreach (var order in orders.OrderBy(x => x.ModifiedDate))
        {
            var store = stores.FirstOrDefault(x => x.Id == order.StoreId);
            if (store == null)
            {
                continue;
            }

            await missionLogicService.ProcessOrderAsync(order, store);
        }
    }
}
