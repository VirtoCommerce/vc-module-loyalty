using System;
using VirtoCommerce.Loyalty.Core.Models.Missions;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyMission : AuditableEntity, ICloneable
{
    /// <summary>
    /// "Draft", "Published", "Archived". A published mission is immutable.
    /// </summary>
    public string Status { get; set; }

    public string Name { get; set; }

    public LocalizedString LocalizedName { get; set; }

    public LocalizedString Description { get; set; }

    /// <summary>
    /// URL of the mission banner image.
    /// </summary>
    public string BannerUrl { get; set; }

    public string StoreId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Whether the mission is exposed to the storefront.
    /// </summary>
    public bool Public { get; set; }

    /// <summary>
    /// Reset periodicity enabler. Currently only "None" (single Start/End window) is processed.
    /// </summary>
    public string Periodicity { get; set; }

    /// <summary>
    /// Qualification conditions + goal node + reward. Mission type and target are derived from the goal node inside this tree.
    /// </summary>
    public LoyaltyMissionConditionAndRewardTree DynamicExpression { get; set; }
        = AbstractTypeFactory<LoyaltyMissionConditionAndRewardTree>.TryCreateInstance();

    public object Clone()
    {
        var result = (LoyaltyMission)MemberwiseClone();

        result.LocalizedName = LocalizedName?.CloneTyped();
        result.Description = Description?.CloneTyped();
        result.DynamicExpression = DynamicExpression?.CloneTyped();

        return result;
    }
}
