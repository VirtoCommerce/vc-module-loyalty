using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyProgramUsageEntity : AuditableEntity, IDataEntity<LoyaltyProgramUsageEntity, LoyaltyProgramUsage>
{
    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string UserId { get; set; }

    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string ObjectId { get; set; }

    [Required]
    [StringLength(DbContextBase.Length128)]
    public string ObjectType { get; set; }

    /// <summary>
    /// Awarded or redeemed
    /// </summary>
    [Required]
    [StringLength(DbContextBase.Length128)]
    public string UsageType { get; set; }

    public decimal Points { get; set; }

    public decimal Balance { get; set; }

    [StringLength(DbContextBase.IdLength)]
    public string LoyaltyProgramId { get; set; }
    public virtual LoyaltyProgramEntity LoyaltyProgram { get; set; }

    public virtual LoyaltyProgramUsage ToModel(LoyaltyProgramUsage model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.UserId = UserId;
        model.LoyaltyProgramId = LoyaltyProgramId;
        model.ObjectId = ObjectId;
        model.ObjectType = ObjectType;
        model.UsageType = UsageType;
        model.Points = Points;
        model.Balance = Balance;

        return model;
    }

    public virtual LoyaltyProgramUsageEntity FromModel(LoyaltyProgramUsage model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        UserId = model.UserId;
        LoyaltyProgramId = model.LoyaltyProgramId;
        ObjectId = model.ObjectId;
        ObjectType = model.ObjectType;
        UsageType = model.UsageType;
        Points = model.Points;
        Balance = model.Balance;

        return this;
    }

    public virtual void Patch(LoyaltyProgramUsageEntity target)
    {

    }
}
