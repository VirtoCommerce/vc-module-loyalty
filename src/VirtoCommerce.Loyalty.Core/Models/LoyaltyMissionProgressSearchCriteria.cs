using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyMissionProgressSearchCriteria : SearchCriteriaBase
{
    public string MissionId { get; set; }

    public string[] MissionIds { get; set; }

    public string UserId { get; set; }

    public string Status { get; set; }

    /// <summary>
    /// Filters progress by the owning mission's store.
    /// </summary>
    public string StoreId { get; set; }
}
