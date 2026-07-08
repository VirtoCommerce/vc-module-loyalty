using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Events;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyMissionService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<LoyaltyMission, LoyaltyMissionEntity, LoyaltyMissionChangingEvent, LoyaltyMissionChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        ILoyaltyMissionService
{
    protected override Task<IList<LoyaltyMissionEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ILoyaltyRepository)repository).GetLoyaltyMissionsByIdsAsync(ids, responseGroup);
    }

    public override async Task SaveChangesAsync(IList<LoyaltyMission> models)
    {
        await EnsureMutableAsync(models);
        await base.SaveChangesAsync(models);
    }

    /// <summary>
    /// A published mission is immutable: the only allowed change is the transition to Archived.
    /// An archived mission is fully immutable. Drafts are freely editable.
    /// </summary>
    private async Task EnsureMutableAsync(IList<LoyaltyMission> models)
    {
        var ids = models.Where(x => !x.Id.IsNullOrEmpty()).Select(x => x.Id).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return;
        }

        var stored = (await GetAsync(ids)).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < models.Count; i++)
        {
            var model = models[i];

            if (model.Id.IsNullOrEmpty() || !stored.TryGetValue(model.Id, out var current))
            {
                continue;
            }

            if (current.Status.EqualsIgnoreCase(ModuleConstants.MissionStatuses.Draft))
            {
                continue;
            }

            var isArchiving = current.Status.EqualsIgnoreCase(ModuleConstants.MissionStatuses.Published)
                && model.Status.EqualsIgnoreCase(ModuleConstants.MissionStatuses.Archived);

            if (!isArchiving)
            {
                throw new InvalidOperationException(
                    $"Mission '{model.Id}' is '{current.Status}' and cannot be modified. Only the transition Published -> Archived is allowed.");
            }

            // Archive only: preserve the published definition, change just the status.
            var archived = current.CloneTyped();
            archived.Status = ModuleConstants.MissionStatuses.Archived;
            models[i] = archived;
        }
    }
}
