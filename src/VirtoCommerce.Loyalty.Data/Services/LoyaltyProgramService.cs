using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Loyalty.Core.Events;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Loyalty.Data.Repositories;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyProgramService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<LoyaltyProgram, LoyaltyProgramEntity, LoyaltyProgramChangingEvent, LoyaltyProgramChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        ILoyaltyProgramService
{
    protected override Task<IList<LoyaltyProgramEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ILoyaltyRepository)repository).GetLoyaltyProgramsByIdsAsync(ids, responseGroup);
    }
}
