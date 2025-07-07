using System;
using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyProgramEntity : AuditableEntity, IDataEntity<LoyaltyProgramEntity, LoyaltyProgram>
{
    public string Name { get; set; }

    public string LocalizedName { get; set; }

    public bool IsActive { get; set; }

    public string StoreId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Priority { get; set; }

    public virtual ObservableCollection<ConditionEntity> Conditions { get; set; } = new NullCollection<ConditionEntity>();

    public ObservableCollection<RewardTypeEntity> RewardTypes { get; set; } = new NullCollection<RewardTypeEntity>();

    public LoyaltyProgramEntity FromModel(LoyaltyProgram model, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(model);

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedDate = model.CreatedDate;
        ModifiedDate = model.ModifiedDate;
        CreatedBy = model.CreatedBy;
        ModifiedBy = model.ModifiedBy;

        StoreId = model.StoreId;
        Name = model.Name;
        LocalizedName = model.LocalizedName;
        IsActive = model.IsActive;
        StartDate = model.StartDate;
        EndDate = model.EndDate;
        Priority = model.Priority;
        if (model.Conditions != null)
        {
            Conditions = [.. model.Conditions.Select(x => AbstractTypeFactory<ConditionEntity>.TryCreateInstance().FromModel(x, pkMap))];
        }
        if (model.RewardTypes != null)
        {
            RewardTypes = [.. model.RewardTypes.Select(x => AbstractTypeFactory<RewardTypeEntity>.TryCreateInstance().FromModel(x, pkMap))];
        }

        return this;
    }

    public void Patch(LoyaltyProgramEntity target)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.StoreId = StoreId;
        target.Name = Name;
        target.LocalizedName = LocalizedName;
        target.IsActive = IsActive;
        target.StartDate = StartDate;
        target.EndDate = EndDate;
        target.Priority = Priority;
        if (!Conditions.IsNullCollection())
        {
            Conditions.Patch(target.Conditions, (sourceContent, targetContent) => sourceContent.Patch(targetContent));
        }
        if (!RewardTypes.IsNullCollection())
        {
            RewardTypes.Patch(target.RewardTypes, (sourceContent, targetContent) => sourceContent.Patch(targetContent));
        }
    }

    public LoyaltyProgram ToModel(LoyaltyProgram model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.Id = Id;
        model.CreatedDate = CreatedDate;
        model.ModifiedDate = ModifiedDate;
        model.CreatedBy = CreatedBy;
        model.ModifiedBy = ModifiedBy;

        model.StoreId = StoreId;
        model.Name = Name;
        model.LocalizedName = LocalizedName;
        model.IsActive = IsActive;
        model.StartDate = StartDate;
        model.EndDate = EndDate;
        model.Priority = Priority;
        model.Conditions = Conditions?.Select(x => x.ToModel(AbstractTypeFactory<Condition>.TryCreateInstance())).ToList();
        model.RewardTypes = RewardTypes?.Select(x => x.ToModel(AbstractTypeFactory<RewardType>.TryCreateInstance())).ToList();

        return model;
    }
}
