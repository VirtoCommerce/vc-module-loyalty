using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.Loyalty.ExperienceApi.Extensions;

public static class DataLoaderContextAccessorExtensions
{

    public static IDataLoaderResult<LoyaltyOperationLogObject> LoadLoyaltyObject(
    this IDataLoaderContextAccessor dataLoader,
    ICustomerOrderService customerOrderService,
    string loaderKey,
    string objectId,
    string objectType)
    {
        var loader = dataLoader.GetDataLoader(customerOrderService, loaderKey);

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

    public static IDataLoader<string, LoyaltyOperationLogObject> GetDataLoader(
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


}
