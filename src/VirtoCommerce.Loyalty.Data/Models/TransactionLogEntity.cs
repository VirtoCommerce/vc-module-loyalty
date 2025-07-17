using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using static VirtoCommerce.Platform.Data.Infrastructure.DbContextBase;

namespace VirtoCommerce.Loyalty.Data.Models;

public class TransactionLogEntity : AuditableEntity, IDataEntity<TransactionLogEntity, TransactionLog>
{
    [Required]
    [StringLength(IdLength)]
    public string LoyaltyProgramId { get; set; }

    [Required]
    [StringLength(IdLength)]
    public string CustomerId { get; set; }

    [Required]
    public LoyaltyOperationType OperationType { get; set; }

    [Required]
    public decimal Points { get; set; }

    [Required]
    [StringLength(IdLength)]
    public string ObjectId { get; set; }

    [Required]
    [StringLength(Length128)]
    public string ObjectType { get; set; }

    public string Comment { get; set; }

    [Required]
    public decimal Balance { get; set; }

    public TransactionLogEntity FromModel(TransactionLog model, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(model);

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedDate = model.CreatedDate;
        ModifiedDate = model.ModifiedDate;
        CreatedBy = model.CreatedBy;
        ModifiedBy = model.ModifiedBy;

        LoyaltyProgramId = model.LoyaltyProgramId;
        CustomerId = model.CustomerId;
        OperationType = model.OperationType;
        Points = model.Points;
        ObjectId = model.ObjectId;
        ObjectType = model.ObjectType;
        Comment = model.Comment;
        Balance = model.Balance;
        return this;
    }

    public void Patch(TransactionLogEntity target)
    {
        ArgumentNullException.ThrowIfNull(target);

        target.LoyaltyProgramId = LoyaltyProgramId;
        target.CustomerId = CustomerId;
        target.OperationType = OperationType;
        target.Points = Points;
        target.ObjectId = ObjectId;
        target.ObjectType = ObjectType;
        target.Comment = Comment;
        target.Balance = Balance;
    }

    public TransactionLog ToModel(TransactionLog model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.Id = Id;
        model.CreatedDate = CreatedDate;
        model.ModifiedDate = ModifiedDate;
        model.CreatedBy = CreatedBy;
        model.ModifiedBy = ModifiedBy;

        model.LoyaltyProgramId = LoyaltyProgramId;
        model.CustomerId = CustomerId;
        model.OperationType = OperationType;
        model.Points = Points;
        model.ObjectId = ObjectId;
        model.ObjectType = ObjectType;
        model.Comment = Comment;
        model.Balance = Balance;

        return model;
    }
}
