using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;

public class LoyaltyBalanceResultType : ExtendableGraphType<LoyaltyBalanceResult>
{
    public LoyaltyBalanceResultType()
    {
        Name = "LoyaltyBalanceResult";
        Description = "Represents the result of a loyalty balance operation.";

        Field(x => x.CurrentBalance).Description("The current balance of the loyalty account.");
        Field(x => x.ResultBance).Description("The resulting balance after applying the operation.");
    }
}
