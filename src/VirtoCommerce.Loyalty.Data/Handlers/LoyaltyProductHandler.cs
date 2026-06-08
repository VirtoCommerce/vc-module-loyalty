using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.DistributedLock;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.Loyalty.Core.ModuleConstants;

namespace VirtoCommerce.Loyalty.Data.Handlers;

public class LoyaltyProductHandler : IEventHandler<OrderChangedEvent>
{
    private readonly IStoreService _storeService;
    private readonly ICustomerOrderService _customerOrderService;
    private readonly ILoyaltyLogicService _loyaltyLogicService;
    private readonly IInternalDistributedLockService _distributedLockProvider;

    public LoyaltyProductHandler(IStoreService storeService,
        ICustomerOrderService customerOrderService,
        ILoyaltyLogicService loyaltyLogicService,
        IInternalDistributedLockService distributedLockProvider)
    {
        _storeService = storeService;
        _customerOrderService = customerOrderService;
        _loyaltyLogicService = loyaltyLogicService;
        _distributedLockProvider = distributedLockProvider;
    }

    public async Task Handle(OrderChangedEvent message)
    {
        var orderIds = message.ChangedEntries
            .Where(x => (x.EntryState == EntryState.Added || x.EntryState == EntryState.Modified))
            .Select(x => x.NewEntry)
            .Where(x => !x.IsPrototype)
            .Select(x => x.Id)
            .ToArray();

        if (orderIds.Length > 0)
        {
            var context = new LoyaltyOrdersContext
            {
                OrderIds = orderIds,
            };

            BackgroundJob.Enqueue(() => ProcessOrdersAsync(context));
        }
    }

    [DisableConcurrentExecution(10)]
    public async Task ProcessOrdersAsync(LoyaltyOrdersContext context)
    {
        var orders = await _customerOrderService.GetNoCloneAsync(context.OrderIds);
        var storeIds = orders.Select(x => x.StoreId).Distinct().ToArray();
        var stores = await _storeService.GetNoCloneAsync(storeIds);

        foreach (var order in orders)
        {
            var store = stores.FirstOrDefault(x => x.Id == order.StoreId);
            if (store == null)
            {
                continue;
            }

            var storeLoyaltyMode = store.Settings.GetValue<string>(Settings.General.LoyaltyMode);
            if (!storeLoyaltyMode.EqualsIgnoreCase("Mixed Cart"))
            {
                continue;
            }

            var storeLoyaltyCurrency = GetLoyaltyCurrencyCode(store);

            var loyaltyTotal = order.OrderTotals?.FirstOrDefault(x => x.CurrencyCode.EqualsIgnoreCase(storeLoyaltyCurrency));
            if (loyaltyTotal == null)
            {
                continue;
            }

            // Evaluate loyalty programs for the context
            var loyaltyAmountResult = AbstractTypeFactory<LoyaltyAmountResult>.TryCreateInstance();
            loyaltyAmountResult.Amount = loyaltyTotal.Total;
            loyaltyAmountResult.OperationType = LoyaltyPrograms.RedeemedOperationType;

            var loyaltyProgramContext = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
            loyaltyProgramContext.ContextObjectType = nameof(CustomerOrder);
            loyaltyProgramContext.OrderId = order.Id;
            loyaltyProgramContext.UserId = order.CustomerId;

            _distributedLockProvider.ExecuteSynchronized($"loyalty-operation:{nameof(CustomerOrder)}:{order.Id}:{LoyaltyPrograms.RedeemedOperationType}", async (x) =>
            {
                await _loyaltyLogicService.LogLoyaltyProgramOperationAsync(loyaltyProgramContext, loyaltyAmountResult);
            });
        }
    }

    private static string GetLoyaltyCurrencyCode(Store store)
    {
        var currencyCode = store.Settings.GetValue<string>(Settings.General.LoyaltyCurrency);
        return !currencyCode.IsNullOrEmpty() ? currencyCode : FallbackLoyaltyCurrencyCode;
    }
}
