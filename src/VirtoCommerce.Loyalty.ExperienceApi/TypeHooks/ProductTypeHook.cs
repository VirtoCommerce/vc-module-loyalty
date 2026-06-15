using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Schemas;
using VirtoCommerce.XCatalog.Core.Models;
using VirtoCommerce.XCatalog.Core.Schemas;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Loyalty.ExperienceApi.TypeHooks;

public class ProductTypeHook : IGraphTypeHook
{
    public string TypeName { get; set; } = "Product";

    public void BeforeTypeInitialized(IGraphType graphType)
    {
        if (graphType is not ProductType productType)
        {
            return;
        }

        var fieldAsync = FieldCreator.CreateFieldAsync<ExpProduct, MoneyType>(
            "loyaltyPoints",
            "Get points amount",
            resolve: async fieldContext =>
            {
                if (fieldContext.Source == null || fieldContext.User.GetCurrentUserId() == AnonymousUser.UserName)
                {
                    return null;
                }

                var dataLoader = fieldContext.RequestServices.GetRequiredService<IDataLoaderContextAccessor>();
                var loader = dataLoader.Context.GetOrAddBatchLoader<ExpProduct, Money>("product_loyalty_points", async products =>
                {
                    var calculator = fieldContext.RequestServices.GetRequiredService<ILoyaltyPointsCalculator>();
                    var pointsContext = await calculator.ResolveAsync(
                        userId: fieldContext.User.GetCurrentUserId(),
                        storeId: fieldContext.GetArgumentOrValue<string>("storeId"),
                        language: fieldContext.GetArgumentOrValue<string>("cultureName"),
                        currencyCode: fieldContext.GetArgumentOrValue<string>("currencyCode"),
                        productIds: products.Select(x => x.Id).Distinct().ToArray());

                    if (pointsContext.PointsCurrency is null)
                    {
                        return new Dictionary<ExpProduct, Money>();
                    }

                    return products.ToDictionary(x => x, x =>
                    {
                        var price = x.AllPrices.FirstOrDefault();
                        return price == null ? null : pointsContext.CalculatePoints(price.ActualPrice.Amount, x.Id);
                    });
                },
                keyComparer: AnonymousComparer.Create((ExpProduct x) => x.Id));

                return loader.LoadAsync(fieldContext.Source);
            });

        productType.AddField(fieldAsync);
    }
}
