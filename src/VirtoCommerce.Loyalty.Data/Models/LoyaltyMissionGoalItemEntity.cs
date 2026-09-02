using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyMissionGoalItemEntity : AuditableEntity, IDataEntity<LoyaltyMissionGoalItemEntity, LoyaltyMissionGoalItem>
{
    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string MissionId { get; set; }
    public virtual LoyaltyMissionEntity Mission { get; set; }

    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual LoyaltyMissionGoalItem ToModel(LoyaltyMissionGoalItem model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.MissionId = MissionId;
        model.ProductId = ProductId;
        model.Quantity = Quantity;

        return model;
    }

    public virtual LoyaltyMissionGoalItemEntity FromModel(LoyaltyMissionGoalItem model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        MissionId = model.MissionId;
        ProductId = model.ProductId;
        Quantity = model.Quantity;

        return this;
    }

    public virtual void Patch(LoyaltyMissionGoalItemEntity target)
    {
        target.MissionId = MissionId;
        target.ProductId = ProductId;
        target.Quantity = Quantity;
    }
}
