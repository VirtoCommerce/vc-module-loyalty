using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgram : AuditableEntity, ICloneable
{
    public string Name { get; set; }

    public string LocalizedName { get; set; }

    public bool IsActive { get; set; }

    public string StoreId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Priority { get; set; }

    public IList<Condition> Conditions { get; set; }

    public IList<RewardType> RewardTypes { get; set; }

    public object Clone()
    {
        var result = (LoyaltyProgram)MemberwiseClone();
        result.Conditions = Conditions?.Select(x => x.CloneTyped()).ToList();
        result.RewardTypes = RewardTypes?.Select(x => x.CloneTyped()).ToList();
        return result;
    }
}
