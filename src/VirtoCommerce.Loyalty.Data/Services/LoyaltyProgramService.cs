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

public class LoyaltyProgramService
    : CrudService<LoyaltyProgram, LoyaltyProgramEntity, LoyaltyProgramChangingEvent, LoyaltyProgramChangedEvent>,
    ILoyaltyProgramService
{
    public LoyaltyProgramService(
        Func<ILoyaltyProgramRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
    }

    protected override Task<IList<LoyaltyProgramEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ILoyaltyProgramRepository)repository).GetLoyaltyProgramsByIdsAsync(ids);
    }
}
