using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgram : AuditableEntity, ICloneable
{
    public bool IsActive { get; set; }

    public string Name { get; set; }

    public LocalizedString LocalizedName { get; set; }

    public string StoreId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Priority { get; set; }

    public LoyaltyProgramConditionAndRewardTree DynamicExpression { get; set; } = AbstractTypeFactory<LoyaltyProgramConditionAndRewardTree>.TryCreateInstance();

    public object Clone()
    {
        var result = (LoyaltyProgram)MemberwiseClone();

        result.LocalizedName = LocalizedName?.CloneTyped();
        result.DynamicExpression = DynamicExpression?.CloneTyped();

        return result;
    }
}
