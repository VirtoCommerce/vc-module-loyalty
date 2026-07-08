using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;

public class LoyaltyMissionProgressItemType : ExtendableGraphType<LoyaltyMissionProgressItem>
{
    public LoyaltyMissionProgressItemType()
    {
        Name = "LoyaltyMissionProgressItem";
        Description = "Per-SKU accumulation for a PerSku mission.";

        Field(x => x.ProductId, nullable: true).Description("The SKU product id.");
        Field(x => x.CurrentQuantity).Description("The accumulated quantity.");
        Field(x => x.TargetQuantity).Description("The target quantity.");
    }
}
