using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Resolvers;
using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;
public class LoyaltyOperationLogType : ExtendableGraphType<LoyaltyProgramOperationLog>
{
    public LoyaltyOperationLogType(
            IDataLoaderContextAccessor dataLoader,
            ICustomerOrderService customerOrderService
        )
    {
        Name = "LoyaltyOperationLog";
        Description = "Represents a log entry for a loyalty program operation.";

        Field(x => x.Id).Description("The unique identifier of the log entry.");
        Field(x => x.OperationType).Description("The type of operation (e.g., Earned, Redeemed).");
        Field(x => x.Amount).Description("The amount involved in the operation.");
        Field(x => x.CreatedDate).Description("The date and time when the log entry was created.");

        var objectField = new FieldType
        {
            Name = "object",
            Type = GraphTypeExtensionHelper.GetActualType<LoyaltyOperationLogObjectType>(),
            Resolver = new FuncFieldResolver<LoyaltyProgramOperationLog, IDataLoaderResult<LoyaltyOperationLogObject>>(context =>
            {
                return dataLoader.LoadLoyaltyObject(customerOrderService,
                    loaderKey: "loyalty_object",
                    objectId: context.Source.ObjectId,
                    objectType: context.Source.ObjectType);
            })
        };
        AddField(objectField);
    }
}

public class LoyaltyOperationLogObject
{
    public string Type { get; set; }

    public string OrderId { get; set; }

    public string OrderNumber { get; set; }
}

public class LoyaltyOperationLogObjectType : ExtendableGraphType<LoyaltyOperationLogObject>
{
    public LoyaltyOperationLogObjectType()
    {
        Name = "LoyaltyOperationLogObject";
        Description = "Represents the object associated with a loyalty program operation log entry.";

        Field(x => x.Type).Description("The type of the object associated with the operation.");

        Field(x => x.OrderId, nullable: true).Description("The identifier of the order associated with the operation, if applicable.");
        Field(x => x.OrderNumber, nullable: true).Description("The number of the order associated with the operation, if applicable.");
    }
}


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
