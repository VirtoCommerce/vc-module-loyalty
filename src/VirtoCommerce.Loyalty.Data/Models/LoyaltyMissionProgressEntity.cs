using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyMissionProgressEntity : AuditableEntity, IDataEntity<LoyaltyMissionProgressEntity, LoyaltyMissionProgress>
{
    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string MissionId { get; set; }
    public virtual LoyaltyMissionEntity Mission { get; set; }

    [Required]
    [StringLength(DbContextBase.IdLength)]
    public string UserId { get; set; }

    public decimal CurrentValue { get; set; }

    public decimal TargetValue { get; set; }

    public decimal Percentage { get; set; }

    [Required]
    [StringLength(DbContextBase.Length128)]
    public string Status { get; set; }

    public DateTime? PeriodStart { get; set; }

    public DateTime? PeriodEnd { get; set; }

    public DateTime? CompletedDate { get; set; }

    public ObservableCollection<LoyaltyMissionProgressItemEntity> Items { get; set; }
        = new NullCollection<LoyaltyMissionProgressItemEntity>();

    public virtual LoyaltyMissionProgress ToModel(LoyaltyMissionProgress model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.MissionId = MissionId;
        model.UserId = UserId;
        model.CurrentValue = CurrentValue;
        model.TargetValue = TargetValue;
        model.Percentage = Percentage;
        model.Status = Status;
        model.PeriodStart = PeriodStart;
        model.PeriodEnd = PeriodEnd;
        model.CompletedDate = CompletedDate;

        if (Items != null)
        {
            model.Items = Items
                .Select(x => new LoyaltyMissionProgressItem
                {
                    MissionProgressId = x.MissionProgressId,
                    ProductId = x.ProductId,
                    CurrentQuantity = x.CurrentQuantity,
                    TargetQuantity = x.TargetQuantity,
                })
                .ToList();
        }

        return model;
    }

    public virtual LoyaltyMissionProgressEntity FromModel(LoyaltyMissionProgress model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        MissionId = model.MissionId;
        UserId = model.UserId;
        CurrentValue = model.CurrentValue;
        TargetValue = model.TargetValue;
        Percentage = model.Percentage;
        Status = model.Status;
        PeriodStart = model.PeriodStart;
        PeriodEnd = model.PeriodEnd;
        CompletedDate = model.CompletedDate;

        if (model.Items != null)
        {
            Items = new ObservableCollection<LoyaltyMissionProgressItemEntity>(model.Items
                .Select(x =>
                {
                    var entity = AbstractTypeFactory<LoyaltyMissionProgressItemEntity>.TryCreateInstance();
                    entity.MissionProgressId = model.Id;
                    entity.ProductId = x.ProductId;
                    entity.CurrentQuantity = x.CurrentQuantity;
                    entity.TargetQuantity = x.TargetQuantity;
                    return entity;
                }));
        }

        return this;
    }

    public virtual void Patch(LoyaltyMissionProgressEntity target)
    {
        target.MissionId = MissionId;
        target.UserId = UserId;
        target.CurrentValue = CurrentValue;
        target.TargetValue = TargetValue;
        target.Percentage = Percentage;
        target.Status = Status;
        target.PeriodStart = PeriodStart;
        target.PeriodEnd = PeriodEnd;
        target.CompletedDate = CompletedDate;

        if (!Items.IsNullCollection())
        {
            var comparer = AnonymousComparer.Create((LoyaltyMissionProgressItemEntity x) => x.ProductId);
            Items.Patch(target.Items, comparer, (source, t) =>
            {
                t.CurrentQuantity = source.CurrentQuantity;
                t.TargetQuantity = source.TargetQuantity;
            });
        }
    }
}
