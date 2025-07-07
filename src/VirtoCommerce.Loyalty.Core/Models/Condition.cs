using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class Condition : AuditableEntity, ICloneable
{
    public string LoyaltyProgramId { get; set; }

    public IList<string> UserGroups { get; set; }

    public bool IsFirstOrder { get; set; }

    public object Clone()
    {
        var result = (Condition)MemberwiseClone();
        result.UserGroups = [.. UserGroups];
        return result;
    }
}
