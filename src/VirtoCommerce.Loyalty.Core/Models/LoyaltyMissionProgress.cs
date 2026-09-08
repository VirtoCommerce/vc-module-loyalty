using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

/// <summary>
/// Summary of a mission (per owner AND per period, where the owner is the user or - in organization
/// balance calculation mode - the organization). It scopes everything downstream: progress items,
/// transactions, the reward record in the operation log.
/// </summary>
public class LoyaltyMissionProgress : AuditableEntity, ICloneable
{
    public string MissionId { get; set; }

    /// <summary>
    /// The user the progress was created for. In organization mode this is the member who
    /// contributed first - the progress itself is shared by the whole organization.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Set only when the store calculates loyalty per organization: all members of the
    /// organization then contribute to (and complete) this single progress.
    /// </summary>
    public string OrganizationId { get; set; }

    /// <summary>
    /// Whoever the progress is scoped to: <see cref="OrganizationId"/> in organization mode,
    /// <see cref="UserId"/> otherwise. Denormalized (instead of indexing the nullable
    /// OrganizationId) because SQL Server, MySQL and PostgreSQL disagree on whether NULLs
    /// collide in a unique index, so only an always-populated column can enforce
    /// "one progress per owner per period" on every provider.
    /// </summary>
    public string OwnerId { get; set; }

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
