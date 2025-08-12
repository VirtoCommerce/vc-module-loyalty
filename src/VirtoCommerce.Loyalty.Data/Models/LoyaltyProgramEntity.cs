using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.Loyalty.Data.Models;

public class LoyaltyProgramEntity : AuditableEntity, IDataEntity<LoyaltyProgramEntity, LoyaltyProgram>
{
    public bool IsActive { get; set; }

    [StringLength(256)]
    public string Name { get; set; }

    [StringLength(128)]
    public string StoreId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Priority { get; set; }

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

        if (PredicateVisualTreeSerialized != null)
        {
            //model.DynamicExpression = JsonConvert.DeserializeObject<LoyaltyProgramConditionAndRewardTree>(PredicateVisualTreeSerialized, new ConditionJsonConverter(), new PolymorphJsonConverter());
            model.DynamicExpression = JsonConvert.DeserializeObject<LoyaltyProgramConditionAndRewardTree>(PredicateVisualTreeSerialized, new ConditionJsonConverter());
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

        if (model.DynamicExpression != null)
        {
            PredicateVisualTreeSerialized = JsonConvert.SerializeObject(model.DynamicExpression, new ConditionJsonConverter(doNotSerializeAvailCondition: true));
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
    }
}
