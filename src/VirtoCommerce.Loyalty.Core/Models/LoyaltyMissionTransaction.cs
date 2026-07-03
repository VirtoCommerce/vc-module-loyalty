using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

/// <summary>
/// Records a single order/event contribution to a mission for a user.
/// Serves as the idempotency gate: a given (MissionId, ObjectId, UserId) is logged once.
/// </summary>
public class LoyaltyMissionTransaction : AuditableEntity, ICloneable
{
    public string MissionId { get; set; }

    /// <summary>
    /// The period-scoped progress this contribution was applied to.
    /// </summary>
    public string MissionProgressId { get; set; }

    public string UserId { get; set; }

    public string ObjectId { get; set; }

    public string ObjectType { get; set; }

    public decimal ContributionValue { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
