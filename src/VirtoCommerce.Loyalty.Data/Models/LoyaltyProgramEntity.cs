using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using static VirtoCommerce.Platform.Data.Infrastructure.DbContextBase;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyProgramEntity : AuditableEntity, IDataEntity<LoyaltyProgramEntity, LoyaltyProgram>
{
    [Required]
    [StringLength(Length128)]
    public string Name { get; set; }

    public ObservableCollection<LoyaltyProgramLocalizedNameEntity> LocalizedNames { get; set; }
        = new NullCollection<LoyaltyProgramLocalizedNameEntity>();

    [Required]
    public bool IsActive { get; set; }

    public virtual ObservableCollection<LoyaltyProgramStoreEntity> Stores { get; set; } = new NullCollection<LoyaltyProgramStoreEntity>();

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public int Priority { get; set; }

    [Required]
    public string Conditions { get; set; }

    public string Code { get; set; }

    public LoyaltyProgramEntity FromModel(LoyaltyProgram model, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(model);

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedDate = model.CreatedDate;
        ModifiedDate = model.ModifiedDate;
        CreatedBy = model.CreatedBy;
        ModifiedBy = model.ModifiedBy;

        if (model.StoreIds != null)
        {
            Stores = [.. model.StoreIds.Select(x => new LoyaltyProgramStoreEntity { StoreId = x, LoyaltyProgramId = model.Id })];
        }
        Name = model.Name;
        IsActive = model.IsActive;
        StartDate = model.StartDate;
        EndDate = model.EndDate;
        Priority = model.Priority;
        Conditions = model.Conditions;
        Code = model.Code;
        if (model.LocalizedName != null)
        {
            LocalizedNames = [.. model.LocalizedName.Values
                .Select(x =>
                {
                    var entity = AbstractTypeFactory<LoyaltyProgramLocalizedNameEntity>.TryCreateInstance();
                    entity.LanguageCode = x.Key;
                    entity.Value = x.Value;
                    return entity;
                })];
        }

        return this;
    }

    public void Patch(LoyaltyProgramEntity target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!Stores.IsNullCollection())
        {
            var comparer = AnonymousComparer.Create((LoyaltyProgramStoreEntity entity) => entity.StoreId);
            Stores.Patch(target.Stores, comparer, (sourceEntity, targetEntity) => targetEntity.StoreId = sourceEntity.StoreId);
        }
        target.Name = Name;
        target.IsActive = IsActive;
        target.StartDate = StartDate;
        target.EndDate = EndDate;
        target.Priority = Priority;
        target.Conditions = Conditions;
        target.Code = Code;

        if (!LocalizedNames.IsNullCollection())
        {
            var localizedNameComparer = AnonymousComparer.Create((LoyaltyProgramLocalizedNameEntity x) => $"{x.Value}-{x.LanguageCode}");
            LocalizedNames.Patch(target.LocalizedNames, localizedNameComparer, (_, _) => { });
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

        if (Stores != null)
        {
            model.StoreIds = [.. Stores.Select(x => x.StoreId)];
        }
        model.Name = Name;
        model.IsActive = IsActive;
        model.StartDate = StartDate;
        model.EndDate = EndDate;
        model.Priority = Priority;
        model.Conditions = Conditions;
        model.Code = Code;
        if (LocalizedNames != null)
        {
            model.LocalizedName = new LocalizedString();
            foreach (var localizedName in LocalizedNames)
            {
                model.LocalizedName.SetValue(localizedName.LanguageCode, localizedName.Value);
            }
        }

        return model;
    }
}
