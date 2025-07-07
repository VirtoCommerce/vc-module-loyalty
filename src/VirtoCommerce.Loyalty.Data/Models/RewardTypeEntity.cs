using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using static VirtoCommerce.Platform.Data.Infrastructure.DbContextBase;

namespace VirtoCommerce.Loyalty.Data.Models;

public class RewardTypeEntity : AuditableEntity, IDataEntity<RewardTypeEntity, RewardType>
{
    [Required]
    [StringLength(IdLength)]
    public string LoyaltyProgramId { get; set; }

    public virtual LoyaltyProgramEntity LoyaltyProgram { get; set; }

    public RewardAmountType AmountType { get; set; }

    public decimal FixedPoints { get; set; }

    public decimal RelativePoints { get; set; }

    public RewardTypeEntity FromModel(RewardType model, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(model);

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedDate = model.CreatedDate;
        ModifiedDate = model.ModifiedDate;
        CreatedBy = model.CreatedBy;
        ModifiedBy = model.ModifiedBy;

        AmountType = model.AmountType;
        FixedPoints = model.FixedPoints;
        RelativePoints = model.RelativePoints;

        return this;
    }

    public void Patch(RewardTypeEntity target)
    {
        ArgumentNullException.ThrowIfNull(target);

        target.AmountType = AmountType;
        target.FixedPoints = FixedPoints;
        target.RelativePoints = RelativePoints;
    }

    public RewardType ToModel(RewardType model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.Id = Id;
        model.CreatedDate = CreatedDate;
        model.ModifiedDate = ModifiedDate;
        model.CreatedBy = CreatedBy;
        model.ModifiedBy = ModifiedBy;

        model.AmountType = AmountType;
        model.FixedPoints = FixedPoints;
        model.RelativePoints = RelativePoints;
        model.LoyaltyProgramId = LoyaltyProgramId;

        return model;
    }
}
