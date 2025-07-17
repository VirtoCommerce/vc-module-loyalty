using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgram : AuditableEntity, ICloneable
{
    public string Name { get; set; }

    public LocalizedString LocalizedName { get; set; }

    public bool IsActive { get; set; } = true;

    public IList<string> StoreIds { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Priority { get; set; }

    public string Conditions { get; set; }

    public string Code { get; set; }

    public object Clone()
    {
        var result = (LoyaltyProgram)MemberwiseClone();
        result.LocalizedName = LocalizedName?.CloneTyped();
        return result;
    }
}
