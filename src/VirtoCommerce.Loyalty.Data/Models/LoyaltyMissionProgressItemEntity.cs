using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyMissionProgressItemEntity : Entity
{
    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string MissionProgressId { get; set; }
    public virtual LoyaltyMissionProgressEntity MissionProgress { get; set; }

    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string ProductId { get; set; }

    public int CurrentQuantity { get; set; }

    public int TargetQuantity { get; set; }
}
