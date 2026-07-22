using System;
using System.Collections.Generic;

namespace VirtoCommerce.Loyalty.Core.Models;

/// <summary>
/// Filters for <see cref="Services.ILoyaltyMissionLogicService.GetUserMissionsAsync"/>.
/// </summary>
public class LoyaltyUserMissionSearchCriteria
{
    public string UserId { get; set; }

    public string StoreId { get; set; }

    /// <summary>
    /// Filter by progress status (InProgress, Completed, Expired). Empty = all.
    /// </summary>
    public IList<string> Statuses { get; set; }

    /// <summary>
    /// Lower bound for the mission CompletedDate filter.
    /// </summary>
    public DateTime? CompletedStartDate { get; set; }

    /// <summary>
    /// Upper bound for the mission CompletedDate filter.
    /// </summary>
    public DateTime? CompletedEndDate { get; set; }

    /// <summary>
    /// Filter by whether the user has started the mission (true = started, false = not started yet).
    /// </summary>
    public bool? IsStarted { get; set; }
}
