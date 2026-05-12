using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Loyalty.ExperienceApi.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Schemas;
using VirtoCommerce.XCart.Core.Schemas;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Loyalty.ExperienceApi.TypeHooks;

public class LineItemTypeHook : IGraphTypeHook
{
    public string TypeName { get; set; } = "LineItemType";

    public void BeforeTypeInitialized(IGraphType graphType)
    {
        if (graphType is not LineItemType lineItemType)
        {
            return;
        }

        var fieldAsync = FieldCreator.CreateFieldAsync<LineItem, MoneyType>(
            "loyaltyPoints",
            "Get points amount",
            resolve: async fieldContext =>
            {
                if (fieldContext.Source == null || fieldContext.User.GetCurrentUserId() == AnonymousUser.UserName)
                {
                    return null;
                }

                var dataLoader = fieldContext.RequestServices.GetRequiredService<IDataLoaderContextAccessor>();
                var loader = dataLoader.Context.GetOrAddBatchLoader<LineItem, Money>("cart_loyalty_points", async lineItems =>
                {
                    var calculator = fieldContext.RequestServices.GetRequiredService<ILoyaltyPointsCalculator>();
                    var pointsContext = await calculator.ResolveAsync(
                        storeId: fieldContext.GetArgumentOrValue<string>("storeId"),
                        userId: fieldContext.User.GetCurrentUserId(),
                        language: fieldContext.GetArgumentOrValue<string>("cultureName"),
                        currencyCode: fieldContext.GetArgumentOrValue<string>("currencyCode"),
                        productIds: lineItems.Select(x => x.ProductId).Distinct().ToArray());

                    if (pointsContext.PointsCurrency is null)
                    {
                        return new Dictionary<LineItem, Money>();
                    }

                    return lineItems.ToDictionary(x => x, x => pointsContext.CalculatePoints(x.ExtendedPrice, x.ProductId));
                },
                keyComparer: AnonymousComparer.Create((LineItem x) => x.Id));

                return loader.LoadAsync(fieldContext.Source);
            });

        lineItemType.AddField(fieldAsync);
    }
}
