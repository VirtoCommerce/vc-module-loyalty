using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyMissionTransactionEntity : AuditableEntity, IDataEntity<LoyaltyMissionTransactionEntity, LoyaltyMissionTransaction>
{
    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string MissionId { get; set; }
    public virtual LoyaltyMissionEntity Mission { get; set; }

    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string MissionProgressId { get; set; }
    public virtual LoyaltyMissionProgressEntity MissionProgress { get; set; }

    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string UserId { get; set; }

    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string ObjectId { get; set; }

    [Required]
    [StringLength(DbContextBase.Length128)]
    public string ObjectType { get; set; }

    public decimal ContributionValue { get; set; }

    public virtual LoyaltyMissionTransaction ToModel(LoyaltyMissionTransaction model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.MissionId = MissionId;
        model.MissionProgressId = MissionProgressId;
        model.UserId = UserId;
        model.ObjectId = ObjectId;
        model.ObjectType = ObjectType;
        model.ContributionValue = ContributionValue;

        return model;
    }

    public virtual LoyaltyMissionTransactionEntity FromModel(LoyaltyMissionTransaction model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        MissionId = model.MissionId;
        MissionProgressId = model.MissionProgressId;
        UserId = model.UserId;
        ObjectId = model.ObjectId;
        ObjectType = model.ObjectType;
        ContributionValue = model.ContributionValue;

        return this;
    }

    public virtual void Patch(LoyaltyMissionTransactionEntity target)
    {
        // append-only, immutable
    }
}
