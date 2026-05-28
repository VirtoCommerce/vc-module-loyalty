using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramProductFactor : AuditableEntity, ICloneable
{
    public string LoyaltyProgramId { get; set; }

    public string ProductId { get; set; }

    public decimal Factor { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
