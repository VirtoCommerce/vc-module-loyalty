using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyProgramEntity : AuditableEntity, IDataEntity<LoyaltyProgramEntity, LoyaltyProgram>
{
    public bool IsActive { get; set; }

    [Required]
    [StringLength(DbContextBase.Length256)]
    public string Name { get; set; }

    public ObservableCollection<LoyaltyProgramLocalizedNameEntity> LocalizedNames { get; set; }
        = new NullCollection<LoyaltyProgramLocalizedNameEntity>();

    [StringLength(DbContextBase.IdLength)]
    public string StoreId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Priority { get; set; }

    /// <summary>
    /// "ProductPoints", "Default"
    /// </summary>
    [StringLength(DbContextBase.Length32)]
    public string ProgramType { get; set; }

    public string PredicateVisualTreeSerialized { get; set; }

    public virtual LoyaltyProgram ToModel(LoyaltyProgram model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.IsActive = IsActive;
        model.Name = Name;
        model.StoreId = StoreId;
        model.StartDate = StartDate;
        model.EndDate = EndDate;
        model.Priority = Priority;
        model.ProgramType = ProgramType;

        if (PredicateVisualTreeSerialized != null)
        {
            model.DynamicExpression = JsonConvert.DeserializeObject<LoyaltyProgramConditionAndRewardTree>(PredicateVisualTreeSerialized, new ConditionJsonConverter());
        }

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

    public virtual LoyaltyProgramEntity FromModel(LoyaltyProgram model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        IsActive = model.IsActive;
        Name = model.Name;
        StoreId = model.StoreId;
        StartDate = model.StartDate;
        EndDate = model.EndDate;
        Priority = model.Priority;
        ProgramType = model.ProgramType;

        if (model.DynamicExpression != null)
        {
            PredicateVisualTreeSerialized = JsonConvert.SerializeObject(model.DynamicExpression, new ConditionJsonConverter(doNotSerializeAvailCondition: true));
        }

        if (model.LocalizedName != null)
        {
            LocalizedNames = new ObservableCollection<LoyaltyProgramLocalizedNameEntity>(model.LocalizedName.Values
                .Select(x =>
                {
                    var entity = AbstractTypeFactory<LoyaltyProgramLocalizedNameEntity>.TryCreateInstance();
                    entity.LanguageCode = x.Key;
                    entity.Value = x.Value;
                    return entity;
                }));
        }

        return this;
    }

    public virtual void Patch(LoyaltyProgramEntity target)
    {
        target.IsActive = IsActive;
        target.Name = Name;
        target.StoreId = StoreId;
        target.StartDate = StartDate;
        target.EndDate = EndDate;
        target.Priority = Priority;
        target.PredicateVisualTreeSerialized = PredicateVisualTreeSerialized;
        target.ProgramType = ProgramType;

        if (!LocalizedNames.IsNullCollection())
        {
            var localizedNameComparer = AnonymousComparer.Create((LoyaltyProgramLocalizedNameEntity x) => $"{x.Value}-{x.LanguageCode}");
            LocalizedNames.Patch(target.LocalizedNames, localizedNameComparer, (sourceValue, targetValue) => { });
        }
    }
}
