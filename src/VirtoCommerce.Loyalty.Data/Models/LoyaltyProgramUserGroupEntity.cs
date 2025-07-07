using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using static VirtoCommerce.Platform.Data.Infrastructure.DbContextBase;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyProgramUserGroupEntity : Entity
{
    [Required]
    [StringLength(IdLength)]
    public string ConditionId { get; set; }

    [Required]
    [StringLength(Length64)]
    public string Group { get; set; }

    public virtual ConditionEntity Condition { get; set; }
}
