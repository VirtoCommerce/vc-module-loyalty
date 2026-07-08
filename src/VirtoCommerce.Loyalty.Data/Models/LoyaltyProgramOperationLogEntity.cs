using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyProgramOperationLogEntity : AuditableEntity, IDataEntity<LoyaltyProgramOperationLogEntity, LoyaltyProgramOperationLog>
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
    /// Earned or redeemed
    /// </summary>
    [Required]
    [StringLength(DbContextBase.Length128)]
    public string OperationType { get; set; }

    public decimal Amount { get; set; }

    public decimal Balance { get; set; }

    /// <summary>
    /// Type of the entity that produced this ledger entry: "LoyaltyProgram" or "LoyaltyMission".
    /// </summary>
    [StringLength(DbContextBase.Length128)]
    public string SourceType { get; set; }

    /// <summary>
    /// Id of the program or mission that produced this ledger entry.
    /// </summary>
    [StringLength(DbContextBase.IdLength)]
    public string SourceId { get; set; }

    public virtual LoyaltyProgramOperationLog ToModel(LoyaltyProgramOperationLog model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.UserId = UserId;
        model.SourceType = SourceType;
        model.SourceId = SourceId;
        model.ObjectId = ObjectId;
        model.ObjectType = ObjectType;
        model.OperationType = OperationType;
        model.Amount = Amount;
        model.Balance = Balance;

        return model;
    }

    public virtual LoyaltyProgramOperationLogEntity FromModel(LoyaltyProgramOperationLog model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        UserId = model.UserId;
        SourceType = model.SourceType;
        SourceId = model.SourceId;
        ObjectId = model.ObjectId;
        ObjectType = model.ObjectType;
        OperationType = model.OperationType;
        Amount = model.Amount;
        Balance = model.Balance;

        return this;
    }

    public virtual void Patch(LoyaltyProgramOperationLogEntity target)
    {
        // intentionally left empty, as this entity is immutable
    }
}
