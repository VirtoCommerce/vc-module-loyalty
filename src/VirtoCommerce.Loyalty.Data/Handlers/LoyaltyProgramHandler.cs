using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Loyalty.Core.Extensions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Provider;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.Loyalty.Core.ModuleConstants;

namespace VirtoCommerce.Loyalty.Data.Handlers;

public class LoyaltyProgramHandler : IEventHandler<OrderChangedEvent>, IEventHandler<UserChangedEvent>
{
    private readonly ILoyaltyLogicService _loyaltyLogicService;
    private readonly IStoreService _storeService;
    private readonly ICustomerOrderService _customerOrderService;
    private readonly ILoyaltyPointsCalculator _loyaltyPointsCalculator;

    public LoyaltyProgramHandler(
        ILoyaltyLogicService loyaltyLogicService,
        IStoreService storeService,
        ICustomerOrderService customerOrderService,
        ILoyaltyPointsCalculator loyaltyPointsCalculator)
    {
        _loyaltyLogicService = loyaltyLogicService;
        _storeService = storeService;
        _customerOrderService = customerOrderService;
        _loyaltyPointsCalculator = loyaltyPointsCalculator;
    }

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
            var context = new LoyaltyOrdersContext
            {
                OrderIds = orderIds,
            };

            BackgroundJob.Enqueue(() => ProcessOrdersAsync(context));
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
    public async Task ProcessOrdersAsync(LoyaltyOrdersContext context)
    {
        var orders = await _customerOrderService.GetNoCloneAsync(context.OrderIds);
        var storeIds = orders.Select(x => x.StoreId).Distinct().ToArray();
        var stores = await _storeService.GetNoCloneAsync(storeIds);

        foreach (var order in orders.OrderBy(x => x.ModifiedDate))
        {
            var store = stores.FirstOrDefault(x => x.Id == order.StoreId);
            if (store == null)
            {
                continue;
            }

            // Balance integrity is guaranteed by the per-user lock inside the loyalty logic
            // service and the unique (object, type, operation) index, so no per-order lock here.
            await ProcessOrderAsync(order, store);
        }
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

            // Balance integrity is guaranteed by the per-user lock inside the loyalty logic
            // service and the unique (object, type, operation) index.
            await _loyaltyLogicService.LogLoyaltyProgramOperationAsync(context, loyaltyReward);
        }
    }

    private async Task ProcessOrderAsync(CustomerOrder order, Store store)
    {
        // Orders that involve the loyalty payment method are handled entirely by the
        // LoyaltyPaymentMethod gateway. Their consistency with the store mode is checked
        // during order validation, so skip them here.
        if (!order.InPayments.IsNullOrEmpty() &&
            order.InPayments.Any(x => x.GatewayCode == nameof(LoyaltyPaymentMethod)))
        {
            return;
        }

        var storeLoyaltyMode = store.Settings.GetValue<string>(Settings.General.LoyaltyMode);
        var isMixedCart = storeLoyaltyMode.EqualsIgnoreCase("Mixed Cart");

        if (isMixedCart)
        {
            // Redeem loyalty-currency total (products bought with loyalty points) and earn
            // via the ProductPoints program (per-item factors) instead of the Default program.
            await EarnProductPointsAsync(order, store);
            await RedeemLoyaltyProductsAsync(order, store);
        }
        else
        {
            await EarnLoyaltyProgramAsync(order);
        }
    }

    private async Task RedeemLoyaltyProductsAsync(CustomerOrder order, Store store)
    {
        var loyaltyCurrency = store.GetLoyaltyCurrencyCode();

        var loyaltyTotal = order.OrderTotals?.FirstOrDefault(x => x.CurrencyCode.EqualsIgnoreCase(loyaltyCurrency));
        if (loyaltyTotal == null || loyaltyTotal.Total <= 0)
        {
            return;
        }

        var loyaltyAmountResult = AbstractTypeFactory<LoyaltyAmountResult>.TryCreateInstance();
        loyaltyAmountResult.Amount = loyaltyTotal.Total;
        loyaltyAmountResult.OperationType = LoyaltyPrograms.RedeemedOperationType;

        var loyaltyContext = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        loyaltyContext.ContextObjectType = nameof(CustomerOrder);
        loyaltyContext.OrderId = order.Id;
        loyaltyContext.UserId = order.CustomerId;

        await _loyaltyLogicService.LogLoyaltyProgramOperationAsync(loyaltyContext, loyaltyAmountResult);
    }

    private async Task EarnProductPointsAsync(CustomerOrder order, Store store)
    {
        // Skip when the order was already awarded (avoids the calculator round-trip).
        if (await _loyaltyLogicService.IsObjectProcessedAsync(nameof(CustomerOrder), order.Id, LoyaltyPrograms.EarnedOperationType))
        {
            return;
        }

        var loyaltyCurrency = store.GetLoyaltyCurrencyCode();

        // Exclude line items already priced in loyalty points (e.g. XPT) - only cash-priced items earn.
        var eligibleItems = order.Items?
            .Where(x => !x.Currency.EqualsIgnoreCase(loyaltyCurrency))
            .ToArray() ?? [];

        if (eligibleItems.Length == 0)
        {
            return;
        }

        var pointsContext = await _loyaltyPointsCalculator.ResolveAsync(
            storeId: order.StoreId,
            userId: order.CustomerId,
            language: order.LanguageCode,
            currencyCode: order.Currency,
            productIds: eligibleItems.Select(x => x.ProductId).Distinct().ToArray());

        if (pointsContext.PointsCurrency == null)
        {
            return;
        }

        var totalPoints = eligibleItems.Sum(x => pointsContext.CalculatePoints(x.ExtendedPrice, x.ProductId)?.Amount ?? 0m);
        if (totalPoints <= 0)
        {
            return;
        }

        var loyaltyAmountResult = AbstractTypeFactory<LoyaltyAmountResult>.TryCreateInstance();
        loyaltyAmountResult.Amount = totalPoints;
        loyaltyAmountResult.OperationType = LoyaltyPrograms.EarnedOperationType;

        var loyaltyContext = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        loyaltyContext.ContextObjectType = nameof(CustomerOrder);
        loyaltyContext.OrderId = order.Id;
        loyaltyContext.UserId = order.CustomerId;

        await _loyaltyLogicService.LogLoyaltyProgramOperationAsync(loyaltyContext, loyaltyAmountResult);
    }

    private async Task EarnLoyaltyProgramAsync(CustomerOrder order)
    {
        // Skip the (potentially expensive) program evaluation when the order was already awarded.
        if (await _loyaltyLogicService.IsObjectProcessedAsync(nameof(CustomerOrder), order.Id, LoyaltyPrograms.EarnedOperationType))
        {
            return;
        }

        var loyaltyContext = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        loyaltyContext.ContextObjectType = nameof(CustomerOrder);
        loyaltyContext.OrderId = order.Id;

        var loyaltyReward = await _loyaltyLogicService.EvaluateLoyaltyProgramsAsync(loyaltyContext);
        if (loyaltyReward == null)
        {
            return;
        }

        await _loyaltyLogicService.LogLoyaltyProgramOperationAsync(loyaltyContext, loyaltyReward);
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
