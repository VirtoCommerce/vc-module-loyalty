using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

public class LoyaltyMissionProgressService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<LoyaltyMissionProgress, LoyaltyMissionProgressEntity, LoyaltyMissionProgressChangingEvent, LoyaltyMissionProgressChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        ILoyaltyMissionProgressService
{
    protected override Task<IList<LoyaltyMissionProgressEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ILoyaltyRepository)repository).GetLoyaltyMissionProgressesByIdsAsync(ids, responseGroup);
    }
}
