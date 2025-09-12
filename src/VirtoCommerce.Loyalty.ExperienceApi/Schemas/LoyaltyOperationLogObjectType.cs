using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;

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
