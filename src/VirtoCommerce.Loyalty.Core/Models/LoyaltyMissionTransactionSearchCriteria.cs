using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyMissionTransactionSearchCriteria : SearchCriteriaBase
{
    public string MissionId { get; set; }

    public string MissionProgressId { get; set; }

    public string UserId { get; set; }

    public string ObjectId { get; set; }
}
