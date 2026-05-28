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

public class LoyaltyProgramProductFactorService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<LoyaltyProgramProductFactor, LoyaltyProgramProductFactorEntity, LoyaltyProgramProductFactorChangingEvent, LoyaltyProgramProductFactorChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        ILoyaltyProgramProductFactorService
{
    protected override Task<IList<LoyaltyProgramProductFactorEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ILoyaltyRepository)repository).GetLoyaltyProgramProductFactorsByIdsAsync(ids, responseGroup);
    }
}
