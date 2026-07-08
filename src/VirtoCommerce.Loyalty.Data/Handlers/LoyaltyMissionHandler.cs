using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
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
            BackgroundJob.Enqueue(() => ProcessMissionsAsync(orderIds));
        }

        return Task.CompletedTask;
    }

    [DisableConcurrentExecution(10)]
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
