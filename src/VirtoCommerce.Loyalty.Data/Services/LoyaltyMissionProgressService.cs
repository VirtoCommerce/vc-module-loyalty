using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Events;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.Platform.Caching;
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

    // NewTransactions rides along in the same SaveChangesAsync call (see LoyaltyMissionProgressEntity.FromModel/Patch),
    // but the base ClearCache only knows about LoyaltyMissionProgress, so the transaction's own cache needs
    // clearing here too - otherwise a caller could read a stale cached "not found" right after this save.
    protected override void ClearCache(IList<LoyaltyMissionProgress> models)
    {
        base.ClearCache(models);

        var transactionIds = models.SelectMany(x => x.NewTransactions).Select(x => x.Id).ToList();
        if (transactionIds.Count > 0)
        {
            foreach (var transactionId in transactionIds)
            {
                GenericCachingRegion<LoyaltyMissionTransaction>.ExpireTokenForKey(transactionId);
            }

            GenericSearchCachingRegion<LoyaltyMissionTransaction>.ExpireRegion();
        }
    }
}
