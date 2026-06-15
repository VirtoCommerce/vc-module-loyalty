using System.Linq;
using FluentValidation;
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
            var loyaltyCurrencyCode = cartValidationContext.CartAggregate.Store.GetLoyaltyCurrencyCode();

            var pointsTotals = cartValidationContext.CartAggregate.Cart.CartTotals.FirstOrDefault(x => x.CurrencyCode.EqualsIgnoreCase(loyaltyCurrencyCode));

            if (pointsTotals == null || pointsTotals.Total <= 0)
            {
                return;
            }

            var cart = cartValidationContext.CartAggregate.Cart;
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
        });
    }
}
