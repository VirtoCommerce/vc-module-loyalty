using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramUsage : AuditableEntity, ICloneable
{
    public string UserId { get; set; }

    public string LoyaltyProgramId { get; set; }

    public string OrderId { get; set; }

    /// <summary>
    /// Awarded or redeemed
    /// </summary>
    public string UsageType { get; set; }

    public decimal Points { get; set; }

    public decimal Balance { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
