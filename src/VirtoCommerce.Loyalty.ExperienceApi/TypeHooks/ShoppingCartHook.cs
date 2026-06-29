using System.Linq;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Loyalty.Core.Extensions;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Schemas;
using VirtoCommerce.XCart.Core;
using VirtoCommerce.XCart.Core.Schemas;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Loyalty.ExperienceApi.TypeHooks;

public class ShoppingCartHook : IGraphTypeHook
{
    public string TypeName { get; set; } = "CartType";

    public void BeforeTypeInitialized(IGraphType graphType)
    {
        if (graphType is not CartType cartType)
        {
            return;
        }

        var fieldAsync = FieldCreator.CreateFieldAsync<CartAggregate, MoneyType>(
            "loyaltyPoints",
            "Get total points amount",
            resolve: async fieldContext =>
            {
                var cartAggregate = fieldContext.Source;
                if (cartAggregate == null || fieldContext.User.GetCurrentUserId() == AnonymousUser.UserName)
                {
                    return null;
                }

                var loyaltyCurrency = cartAggregate.Store.GetLoyaltyCurrencyCode();

                var eligibleItems = cartAggregate.SelectedLineItems
                    .Where(x => !x.Currency.EqualsIgnoreCase(loyaltyCurrency))
                    .ToArray();

                var calculator = fieldContext.RequestServices.GetRequiredService<ILoyaltyPointsCalculator>();
                var pointsContext = await calculator.ResolveAsync(
                    userId: fieldContext.User.GetCurrentUserId(),
                    storeId: fieldContext.GetArgumentOrValue<string>("storeId"),
                    language: fieldContext.GetArgumentOrValue<string>("cultureName"),
                    currencyCode: fieldContext.GetArgumentOrValue<string>("currencyCode"),
                    productIds: eligibleItems.Select(x => x.ProductId).Distinct().ToArray());


                var totalPoints = new Money(0.0m, pointsContext.PointsCurrency);
                foreach (var lineItem in eligibleItems)
                {
                    var points = pointsContext.CalculatePoints(lineItem.ExtendedPrice, lineItem.ProductId);
                    totalPoints += points;
                }

                return totalPoints;
            });

        cartType.AddField(fieldAsync);
    }
}
