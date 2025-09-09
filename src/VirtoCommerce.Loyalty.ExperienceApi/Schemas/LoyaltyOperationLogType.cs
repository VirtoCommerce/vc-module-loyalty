using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;
public class LoyaltyOperationLogType : ExtendableGraphType<LoyaltyProgramOperationLog>
{
    public LoyaltyOperationLogType()
    {
        Name = "LoyaltyOperationLog";
        Description = "Represents a log entry for a loyalty program operation.";

        Field(x => x.Id).Description("The unique identifier of the log entry.");
        Field(x => x.UserId).Description("The identifier of the user associated with the operation.");
        Field(x => x.ObjectId).Description("The identifier of the object associated with the operation.");
        Field(x => x.ObjectType).Description("The type of the object associated with the operation.");
        Field(x => x.OperationType).Description("The type of operation (e.g., Earned, Redeemed).");
        Field(x => x.Amount).Description("The amount involved in the operation.");
        Field(x => x.CreatedDate).Description("The date and time when the log entry was created.");
    }
}
