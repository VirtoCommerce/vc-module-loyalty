using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyProgramProductFactorEntity : AuditableEntity, IDataEntity<LoyaltyProgramProductFactorEntity, LoyaltyProgramProductFactor>
{
    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string LoyaltyProgramId { get; set; }
    public virtual LoyaltyProgramEntity LoyaltyProgram { get; set; }


    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string ProductId { get; set; }

    public decimal Factor { get; set; }

    public virtual LoyaltyProgramProductFactor ToModel(LoyaltyProgramProductFactor model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.ProductId = ProductId;
        model.LoyaltyProgramId = LoyaltyProgramId;
        model.Factor = Factor;

        return model;
    }

    public virtual LoyaltyProgramProductFactorEntity FromModel(LoyaltyProgramProductFactor model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        ProductId = model.ProductId;
        LoyaltyProgramId = model.LoyaltyProgramId;
        Factor = model.Factor;

        return this;
    }

    public virtual void Patch(LoyaltyProgramProductFactorEntity target)
    {
        target.ProductId = ProductId;
        target.LoyaltyProgramId = LoyaltyProgramId;
        target.Factor = Factor;
    }
}
