using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

/// <summary>
/// Summary of a mission (Per-user AND Per-period). It scopes everything downstream:
/// progress items, transactions, the reward record in the operation log.
/// </summary>
public class LoyaltyMissionProgress : AuditableEntity, ICloneable
{
    public string MissionId { get; set; }

    public string UserId { get; set; }

    public decimal CurrentValue { get; set; }

    public decimal TargetValue { get; set; }

    public decimal Percentage { get; set; }

    /// <summary>
    /// "InProgress", "Completed", "Expired".
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Occurrence window. For "None" periodicity equals the mission Start/End.
    /// </summary>
    public DateTime? PeriodStart { get; set; }

    public DateTime? PeriodEnd { get; set; }

    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Progress items for PerSku missions.
    /// </summary>
    public IList<LoyaltyMissionProgressItem> Items { get; set; } = [];

    /// <summary>
    /// Transient write buffer: transactions to persist alongside this progress in the same
    /// SaveChangesAsync call (see LoyaltyMissionProgressEntity.FromModel/Patch). Cleared by the
    /// caller once saved - it does not reflect the progress's full transaction history.
    /// </summary>
    public IList<LoyaltyMissionTransaction> NewTransactions { get; set; } = [];

    public object Clone()
    {
        var result = (LoyaltyMissionProgress)MemberwiseClone();

        result.Items = Items?.Select(x => x.Clone()).OfType<LoyaltyMissionProgressItem>().ToList();
        result.NewTransactions = NewTransactions?.Select(x => x.Clone()).OfType<LoyaltyMissionTransaction>().ToList();

        return result;
    }
}
