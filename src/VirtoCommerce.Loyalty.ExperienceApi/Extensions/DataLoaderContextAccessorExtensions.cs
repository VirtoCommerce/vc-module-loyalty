using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.XCatalog.Core.Models;
using VirtoCommerce.XCatalog.Core.Queries;

namespace VirtoCommerce.Loyalty.ExperienceApi.Extensions;

public static class DataLoaderContextAccessorExtensions
{

    private static readonly DataLoaderResult<ExpProduct> _defaultProductResult = new((ExpProduct)null);

    public static IDataLoaderResult<LoyaltyOperationLogObject> LoadLoyaltyObject(
    this IDataLoaderContextAccessor dataLoader,
    ICustomerOrderService customerOrderService,
    string loaderKey,
    string objectId,
    string objectType)
    {
        var loader = dataLoader.GetLoyaltyOrderDataLoader(customerOrderService, loaderKey);

        return objectType switch
        {
            nameof(CustomerOrder) => loader.LoadAsync(objectId),
            nameof(ApplicationUser) => new DataLoaderResult<LoyaltyOperationLogObject>(Task.FromResult(new LoyaltyOperationLogObject
            {
                Type = "Registration",
            })),
            _ => new DataLoaderResult<LoyaltyOperationLogObject>(Task.FromResult<LoyaltyOperationLogObject>(null))
        };
    }

    public static IDataLoader<string, LoyaltyOperationLogObject> GetLoyaltyOrderDataLoader(
        this IDataLoaderContextAccessor dataLoader,
        ICustomerOrderService customerOrderService,
        string loaderKey)
    {
        var loader = dataLoader.Context.GetOrAddBatchLoader<string, LoyaltyOperationLogObject>(loaderKey, async (ids) =>
        {
            var result = new Dictionary<string, LoyaltyOperationLogObject>();

            var orders = await customerOrderService.GetAsync(ids.ToArray(), responseGroup: CustomerOrderResponseGroup.Default.ToString(), clone: false);
            foreach (var order in orders)
            {
                result.Add(order.Id, new LoyaltyOperationLogObject
                {
                    Type = nameof(CustomerOrder),
                    OrderId = order.Id,
                    OrderNumber = order.Number,
                });
            }

            return result;
        });

        return loader;
    }

    public static IDataLoaderResult<ExpProduct> LoadProduct(
        this IDataLoaderContextAccessor dataLoader,
        IResolveFieldContext<LoyaltyMissionProgressItem> context,
        string loaderKey,
        string productId)
    {
        if (string.IsNullOrEmpty(productId))
        {
            return _defaultProductResult;
        }

        var mediator = context.RequestServices.GetRequiredService<IMediator>();
        var currencyService = context.RequestServices.GetRequiredService<ICurrencyService>();

        var loader = dataLoader.GetProductDataLoader(context, mediator, currencyService, loaderKey);

        return loader.LoadAsync(productId);
    }

    public static IDataLoader<string, ExpProduct> GetProductDataLoader(
        this IDataLoaderContextAccessor dataLoader,
        IResolveFieldContext<LoyaltyMissionProgressItem> context,
        IMediator mediator,
        ICurrencyService currencyService,
        string loaderKey)
    {
        var loader = dataLoader.Context.GetOrAddBatchLoader<string, ExpProduct>(loaderKey, async ids =>
        {
            var mission = context.GetValue<LoyaltyUserMission>(context.Source.MissionId);
            if (mission == null)
            {
                return new Dictionary<string, ExpProduct>();
            }

            var userId = context.GetArgumentOrValue<string>("userId");

            var request = new LoadProductsQuery
            {
                ObjectIds = ids.ToArray(),
                StoreId = mission.Store.Id,
                CurrencyCode = mission.MissionCurrencyCode,
                IncludeFields = context.SubFields.Values.GetAllNodesPaths(context).ToArray(),
                UserId = userId,
                OrganizationId = context.GetCurrentOrganizationId(),
            };

            var allCurrencies = await currencyService.GetAllCurrenciesAsync();
            var cultureName = context.GetArgumentOrValue<string>("cultureName");
            context.SetCurrencies(allCurrencies, cultureName);

            context.UserContext.TryAdd("currencyCode", mission.MissionCurrencyCode);
            context.UserContext.TryAdd("storeId", mission.Store.Id);
            context.UserContext.TryAdd("cultureName", cultureName);

            var response = await mediator.Send(request);

            return response.Products.ToDictionary(x => x.Id);
        });

        return loader;
    }


}
