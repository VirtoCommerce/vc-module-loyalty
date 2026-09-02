using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.ExperienceApi.Validators;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.XCart.Core;
using VirtoCommerce.XCart.Core.Validators;
using Xunit;
using ModuleConstants = VirtoCommerce.Loyalty.Core.ModuleConstants;

namespace VirtoCommerce.Loyalty.Tests;

[Trait("Category", "Unit")]
public class LoyaltyCartValidatorTests
{
    private const string LoyaltyCurrency = "PTS";
    private const string CashCurrency = "USD";
    private const string OnlyPointProductsErrorCode = "LOYALTY_ONLY_POINT_PRODUCTS_NOT_ALLOWED";

    [Fact]
    public async Task ValidateAsync_MixedCart_AllCashLinesDeselected_ReportsOnlyPointProductsError()
    {
        var cart = CreateCart(
            CreateLineItem(CashCurrency, selectedForCheckout: false),
            CreateLineItem(LoyaltyCurrency, selectedForCheckout: true));

        // Totals are summed from selected items only, so the deselected cash line contributes nothing.
        cart.CartTotals =
        [
            new CartTotal { CurrencyCode = CashCurrency, Total = 0m },
            new CartTotal { CurrencyCode = LoyaltyCurrency, Total = 6m },
        ];

        var validator = CreateValidator();

        var result = await validator.ValidateAsync(CreateContext(cart), TestContext.Current.CancellationToken);

        Assert.Contains(result.Errors, x => x.ErrorCode == OnlyPointProductsErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_MixedCart_CashLineSelected_DoesNotReportOnlyPointProductsError()
    {
        var cart = CreateCart(
            CreateLineItem(CashCurrency, selectedForCheckout: true),
            CreateLineItem(LoyaltyCurrency, selectedForCheckout: true));

        cart.CartTotals =
        [
            new CartTotal { CurrencyCode = CashCurrency, Total = 25m },
            new CartTotal { CurrencyCode = LoyaltyCurrency, Total = 6m },
        ];

        var validator = CreateValidator();

        var result = await validator.ValidateAsync(CreateContext(cart), TestContext.Current.CancellationToken);

        Assert.DoesNotContain(result.Errors, x => x.ErrorCode == OnlyPointProductsErrorCode);
    }

    // Gifts are excluded from totals, so counting one as cash would misalign the two operands again.
    // Not reachable via the storefront today (ApplyRewardsAsync drops unmatched gifts on recalculation);
    // this pins the semantics for direct callers of the validator.
    [Fact]
    public async Task ValidateAsync_MixedCart_GiftLineIsNotCountedAsCashProduct()
    {
        var cart = CreateCart(
            CreateLineItem(CashCurrency, selectedForCheckout: true, isGift: true),
            CreateLineItem(LoyaltyCurrency, selectedForCheckout: true));

        cart.CartTotals = [new CartTotal { CurrencyCode = LoyaltyCurrency, Total = 6m }];

        var validator = CreateValidator();

        var result = await validator.ValidateAsync(CreateContext(cart), TestContext.Current.CancellationToken);

        Assert.Contains(result.Errors, x => x.ErrorCode == OnlyPointProductsErrorCode);
    }

    private static LoyaltyCartValidator CreateValidator() => new(new FakeLoyaltyLogicService(balance: 1000m));

    private static ShoppingCart CreateCart(params LineItem[] items) => new()
    {
        Id = "cart-1",
        CustomerId = "customer-1",
        Currency = CashCurrency,
        Items = items.ToList(),
        Payments = [],
        CartTotals = [],
    };

    private static LineItem CreateLineItem(string currency, bool selectedForCheckout, bool isGift = false) => new()
    {
        Id = $"item-{currency}{(isGift ? "-gift" : string.Empty)}",
        ProductId = $"product-{currency}",
        Quantity = 1,
        Currency = currency,
        SelectedForCheckout = selectedForCheckout,
        IsGift = isGift,
    };

    private static CartValidationContext CreateContext(ShoppingCart cart)
    {
        var store = new Store
        {
            Id = "store-1",
            Settings =
            [
                CreateSetting(ModuleConstants.Settings.General.LoyaltyMode, ModuleConstants.LoyaltyModes.MixedCart),
                CreateSetting(ModuleConstants.Settings.General.LoyaltyCurrency, LoyaltyCurrency),
            ],
        };

        var aggregate = new TestCartAggregate();
        aggregate.Setup(cart, store);

        return new CartValidationContext { CartAggregate = aggregate };
    }

    private static ObjectSettingEntry CreateSetting(SettingDescriptor descriptor, object value) =>
        new(descriptor) { Value = value };

    // Cart/Store have protected setters; the nulls match the pinned VirtoCommerce.XCart 3.1023.0
    // constructor (a later version adds ICartItemBuilder, which will break this at compile time).
    private sealed class TestCartAggregate() : CartAggregate(null, null, null, null, null, null, null, null, null, null, null, null)
    {
        public void Setup(ShoppingCart cart, Store store)
        {
            Cart = cart;
            Store = store;
        }
    }

    // Only GetUserBalanceAsync is reached; the rest throw so an unexpected call fails loudly.
    private sealed class FakeLoyaltyLogicService(decimal balance) : ILoyaltyLogicService
    {
        public Task<decimal> GetUserBalanceAsync(string userId) => Task.FromResult(balance);

        public Task<LoyaltyBalanceResult> GetLoyaltyBalanceAsync(LoyaltyBalanceRequest request) =>
            throw new NotSupportedException();

        public Task<bool> IsObjectProcessedAsync(string objectType, string objectId, string operationType) =>
            throw new NotSupportedException();

        public Task<List<string>> FindProcessedObjectIdsAsync(string objectType, string[] objectIds) =>
            throw new NotSupportedException();

        public Task<LoyaltyAmountResult> EvaluateLoyaltyProgramsAsync(LoyaltyProgramEvaluationContext loyaltyContext) =>
            throw new NotSupportedException();

        public Task<bool> LogLoyaltyProgramOperationAsync(LoyaltyProgramEvaluationContext loyaltyContext, LoyaltyAmountResult loyaltyResult) =>
            throw new NotSupportedException();

        public Task PopulateLoyaltyProgramEvaluationContextAsync(LoyaltyProgramEvaluationContext context) =>
            throw new NotImplementedException();
    }
}
