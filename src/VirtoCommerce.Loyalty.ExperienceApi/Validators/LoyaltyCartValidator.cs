using System.Linq;
using FluentValidation;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Extensions;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.XCart.Core.Models;
using VirtoCommerce.XCart.Core.Validators;

namespace VirtoCommerce.Loyalty.ExperienceApi.Validators;

public class LoyaltyCartValidator : AbstractValidator<CartValidationContext>, ICartValidator<CartValidationContext>
{
    public int Order => 100;

    public LoyaltyCartValidator(ILoyaltyLogicService loyaltyService)
    {
        RuleFor(x => x).CustomAsync(async (cartValidationContext, context, _) =>
        {
            var store = cartValidationContext.CartAggregate.Store;
            var cart = cartValidationContext.CartAggregate.Cart;

            var loyaltyMode = store.GetLoyaltyMode();
            var loyaltyCurrencyCode = store.GetLoyaltyCurrencyCode();

            var pointsTotals = cart.CartTotals.FirstOrDefault(x => x.CurrencyCode.EqualsIgnoreCase(loyaltyCurrencyCode));
            var hasPointProducts = pointsTotals != null && pointsTotals.Total > 0;

            var usesLoyaltyPayment = cart.Payments?.Any(x => x.PaymentGatewayCode.EqualsIgnoreCase(ModuleConstants.LoyaltyPaymentMethodGatewayCode)) == true;

            // Products priced in loyalty points are only valid in Mixed Cart mode.
            if (hasPointProducts && !loyaltyMode.EqualsIgnoreCase(ModuleConstants.LoyaltyModes.MixedCart))
            {
                context.AddFailure(new CartValidationError(cart,
                    "Loyalty point products are not allowed for the current store loyalty mode", "LOYALTY_POINT_PRODUCTS_NOT_ALLOWED"));
            }

            // The loyalty payment method is only valid in Payment Method mode.
            if (usesLoyaltyPayment && !loyaltyMode.EqualsIgnoreCase(ModuleConstants.LoyaltyModes.PaymentMethod))
            {
                context.AddFailure(new CartValidationError(cart,
                    "Loyalty payment method is not allowed for the current store loyalty mode", "LOYALTY_PAYMENT_METHOD_NOT_ALLOWED"));
            }

            // Ensure the balance covers the points spent on loyalty-priced products.
            if (hasPointProducts)
            {
                var balance = await loyaltyService.GetUserBalanceAsync(cart.CustomerId);
                if (balance < pointsTotals.Total)
                {
                    context.AddFailure(new CartValidationError(cart,
                        "Insufficient loyalty points balance", "LOYALTY_INSUFFICIENT_BALANCE")
                    {
                        FormattedMessagePlaceholderValues = new()
                        {
                            ["required"] = pointsTotals.Total,
                            ["available"] = balance,
                        }
                    });
                }
            }
        });
    }
}
