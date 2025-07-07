using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class RewardType : AuditableEntity, ICloneable
{
    public string LoyaltyProgramId { get; set; }
    
    public RewardAmountType AmountType { get; set; }

    public decimal FixedPoints { get; set; }

    public decimal RelativePoints { get; set; }

    public object Clone() => (RewardType)MemberwiseClone();
}
