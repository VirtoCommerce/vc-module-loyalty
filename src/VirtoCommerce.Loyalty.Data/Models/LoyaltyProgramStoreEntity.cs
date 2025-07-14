using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using static VirtoCommerce.Platform.Data.Infrastructure.DbContextBase;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyProgramStoreEntity : Entity
{
    public string LoyaltyProgramId { get; set; }

    public virtual LoyaltyProgramEntity LoyaltyProgram { get; set; }

    [StringLength(IdLength)]
    [Required]
    public string StoreId { get; set; }
}
