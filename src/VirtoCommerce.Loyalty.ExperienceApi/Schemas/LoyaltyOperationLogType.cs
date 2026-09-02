using GraphQL.DataLoader;
using GraphQL.Resolvers;
using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.ExperienceApi.Extensions;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;
public class LoyaltyOperationLogType : ExtendableGraphType<LoyaltyBalanceOperationLog>
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
            Resolver = new FuncFieldResolver<LoyaltyBalanceOperationLog, IDataLoaderResult<LoyaltyOperationLogObject>>(context =>
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
