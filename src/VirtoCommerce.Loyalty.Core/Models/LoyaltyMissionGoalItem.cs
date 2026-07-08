using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

/// <summary>
/// A single SKU target of a PerSku mission: buy Quantity units of ProductId.
/// </summary>
public class LoyaltyMissionGoalItem : AuditableEntity, ICloneable
{
    public string MissionId { get; set; }

    public string ProductId { get; set; }

    public int Quantity { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
