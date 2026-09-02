using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyMissionSearchService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ILoyaltyMissionService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<LoyaltyMissionSearchCriteria, LoyaltyMissionSearchResult, LoyaltyMission, LoyaltyMissionEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ILoyaltyMissionSearchService
{
    protected override IQueryable<LoyaltyMissionEntity> BuildQuery(IRepository repository, LoyaltyMissionSearchCriteria criteria)
    {
        var query = ((ILoyaltyRepository)repository).LoyaltyMissions;

        if (criteria.OnlyActive)
        {
            var now = DateTime.UtcNow;
            query = query.Where(x => x.Status == ModuleConstants.MissionStatuses.Published
                && (x.StartDate == null || x.StartDate <= now)
                && (x.EndDate == null || x.EndDate >= now));
        }

        if (!criteria.Status.IsNullOrEmpty())
        {
            query = query.Where(x => x.Status == criteria.Status);
        }

        if (criteria.Public != null)
        {
            query = query.Where(x => x.Public == criteria.Public.Value);
        }

        if (!criteria.Keyword.IsNullOrEmpty())
        {
            query = query.Where(x => x.Name.Contains(criteria.Keyword));
        }

        if (!criteria.StoreIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.StoreIds.Contains(x.StoreId));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyMissionSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(LoyaltyMissionEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(LoyaltyMissionEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
