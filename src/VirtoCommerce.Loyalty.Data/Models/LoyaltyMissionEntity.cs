using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Models.Missions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyMissionEntity : AuditableEntity, IDataEntity<LoyaltyMissionEntity, LoyaltyMission>
{
    [Required]
    [StringLength(DbContextBase.Length128)]
    public string Status { get; set; }

    [Required]
    [StringLength(DbContextBase.Length256)]
    public string Name { get; set; }

    public ObservableCollection<LoyaltyMissionLocalizedNameEntity> LocalizedNames { get; set; }
        = new NullCollection<LoyaltyMissionLocalizedNameEntity>();

    public ObservableCollection<LoyaltyMissionLocalizedDescriptionEntity> LocalizedDescriptions { get; set; }
        = new NullCollection<LoyaltyMissionLocalizedDescriptionEntity>();

    [StringLength(DbContextBase.IdLength)]
    public string StoreId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool Public { get; set; }

    [StringLength(DbContextBase.Length128)]
    public string Periodicity { get; set; }

    public string PredicateVisualTreeSerialized { get; set; }

    public virtual LoyaltyMission ToModel(LoyaltyMission model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.Status = Status;
        model.Name = Name;
        model.StoreId = StoreId;
        model.StartDate = StartDate;
        model.EndDate = EndDate;
        model.Public = Public;
        model.Periodicity = Periodicity;

        if (PredicateVisualTreeSerialized != null)
        {
            model.DynamicExpression = JsonConvert.DeserializeObject<LoyaltyMissionConditionAndRewardTree>(PredicateVisualTreeSerialized, new ConditionJsonConverter());
        }

        if (LocalizedNames != null)
        {
            model.LocalizedName = new LocalizedString();
            foreach (var localizedName in LocalizedNames)
            {
                model.LocalizedName.SetValue(localizedName.LanguageCode, localizedName.Value);
            }
        }

        if (LocalizedDescriptions != null)
        {
            model.Description = new LocalizedString();
            foreach (var localizedDescription in LocalizedDescriptions)
            {
                model.Description.SetValue(localizedDescription.LanguageCode, localizedDescription.Value);
            }
        }

        return model;
    }

    public virtual LoyaltyMissionEntity FromModel(LoyaltyMission model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        Status = model.Status;
        Name = model.Name;
        StoreId = model.StoreId;
        StartDate = model.StartDate;
        EndDate = model.EndDate;
        Public = model.Public;
        Periodicity = model.Periodicity;

        if (model.DynamicExpression != null)
        {
            PredicateVisualTreeSerialized = JsonConvert.SerializeObject(model.DynamicExpression, new ConditionJsonConverter(doNotSerializeAvailCondition: true));
        }

        if (model.LocalizedName != null)
        {
            LocalizedNames = new ObservableCollection<LoyaltyMissionLocalizedNameEntity>(model.LocalizedName.Values
                .Select(x =>
                {
                    var entity = AbstractTypeFactory<LoyaltyMissionLocalizedNameEntity>.TryCreateInstance();
                    entity.LanguageCode = x.Key;
                    entity.Value = x.Value;
                    return entity;
                }));
        }

        if (model.Description != null)
        {
            LocalizedDescriptions = new ObservableCollection<LoyaltyMissionLocalizedDescriptionEntity>(model.Description.Values
                .Select(x =>
                {
                    var entity = AbstractTypeFactory<LoyaltyMissionLocalizedDescriptionEntity>.TryCreateInstance();
                    entity.LanguageCode = x.Key;
                    entity.Value = x.Value;
                    return entity;
                }));
        }

        return this;
    }

    public virtual void Patch(LoyaltyMissionEntity target)
    {
        target.Status = Status;
        target.Name = Name;
        target.StoreId = StoreId;
        target.StartDate = StartDate;
        target.EndDate = EndDate;
        target.Public = Public;
        target.Periodicity = Periodicity;
        target.PredicateVisualTreeSerialized = PredicateVisualTreeSerialized;

        if (!LocalizedNames.IsNullCollection())
        {
            var comparer = AnonymousComparer.Create((LoyaltyMissionLocalizedNameEntity x) => $"{x.Value}-{x.LanguageCode}");
            LocalizedNames.Patch(target.LocalizedNames, comparer, (sourceValue, targetValue) => { });
        }

        if (!LocalizedDescriptions.IsNullCollection())
        {
            var comparer = AnonymousComparer.Create((LoyaltyMissionLocalizedDescriptionEntity x) => $"{x.Value}-{x.LanguageCode}");
            LocalizedDescriptions.Patch(target.LocalizedDescriptions, comparer, (sourceValue, targetValue) => { });
        }
    }
}
