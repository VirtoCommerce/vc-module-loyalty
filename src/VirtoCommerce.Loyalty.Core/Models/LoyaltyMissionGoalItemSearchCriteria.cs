using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyMissionGoalItemSearchCriteria : SearchCriteriaBase
{
    public string MissionId { get; set; }

    public IList<string> MissionIds { get; set; }

    public IList<string> ProductIds { get; set; }
}
