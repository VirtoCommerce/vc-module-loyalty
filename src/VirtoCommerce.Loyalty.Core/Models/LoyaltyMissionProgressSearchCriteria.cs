using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyMissionProgressSearchCriteria : SearchCriteriaBase
{
    public string MissionId { get; set; }

    public string[] MissionIds { get; set; }

    public string UserId { get; set; }

    public string OrganizationId { get; set; }

    /// <summary>
    /// Filters by the progress owner (organization in organization mode, user otherwise).
    /// Prefer it over <see cref="UserId"/> when looking up the single progress a contribution
    /// belongs to - <see cref="UserId"/> also matches organization-scoped rows.
    /// </summary>
    public string OwnerId { get; set; }

    public string Status { get; set; }

    private IList<string> _statuses;
    public IList<string> Statuses
    {
        get
        {
            if (_statuses.IsNullOrEmpty() && !string.IsNullOrEmpty(Status))
            {
                _statuses = [Status];
            }
            return _statuses;
        }
        set
        {
            _statuses = value;
        }
    }

    /// <summary>
    /// Filters progress by the owning mission's store.
    /// </summary>
    public string StoreId { get; set; }
}
