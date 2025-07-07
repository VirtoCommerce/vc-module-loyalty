using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using static VirtoCommerce.Platform.Data.Infrastructure.DbContextBase;

namespace VirtoCommerce.Loyalty.Data.Models;

public class ConditionEntity : AuditableEntity, IDataEntity<ConditionEntity, Condition>
{
    [Required]
    [StringLength(IdLength)]
    public string LoyaltyProgramId { get; set; }

    public virtual LoyaltyProgramEntity LoyaltyProgram { get; set; }

    public virtual ObservableCollection<LoyaltyProgramUserGroupEntity> UserGroups { get; set; } = new NullCollection<LoyaltyProgramUserGroupEntity>();

    public bool IsFirstOrder { get; set; }

    public ConditionEntity FromModel(Condition model, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(model);

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedDate = model.CreatedDate;
        ModifiedDate = model.ModifiedDate;
        CreatedBy = model.CreatedBy;
        ModifiedBy = model.ModifiedBy;

        if (model.UserGroups != null)
        {
            UserGroups = [];

            foreach (var group in model.UserGroups)
            {
                var userGroupEntity = AbstractTypeFactory<LoyaltyProgramUserGroupEntity>.TryCreateInstance();
                userGroupEntity.Group = group;
                userGroupEntity.ConditionId = model.Id;

                UserGroups.Add(userGroupEntity);
            }
        }
        IsFirstOrder = model.IsFirstOrder;

        return this;
    }

    public void Patch(ConditionEntity target)
    {
        ArgumentNullException.ThrowIfNull(target);

        target.IsFirstOrder = IsFirstOrder;
        if (!UserGroups.IsNullCollection())
        {
            var userGroupComparer = AnonymousComparer.Create((LoyaltyProgramUserGroupEntity x) => x.Group);
            UserGroups.Patch(target.UserGroups, userGroupComparer, (sourceGroup, targetGroup) => targetGroup.Group = sourceGroup.Group);
        }
    }

    public Condition ToModel(Condition model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.Id = Id;
        model.CreatedDate = CreatedDate;
        model.ModifiedDate = ModifiedDate;
        model.CreatedBy = CreatedBy;
        model.ModifiedBy = ModifiedBy;

        model.UserGroups = [.. UserGroups.OrderBy(x => x.Group).Select(x => x.Group)];
        model.IsFirstOrder = IsFirstOrder;
        model.LoyaltyProgramId = LoyaltyProgramId;

        return model;
    }
}
