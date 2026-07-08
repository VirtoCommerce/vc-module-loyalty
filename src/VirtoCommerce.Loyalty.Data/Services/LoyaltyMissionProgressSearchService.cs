using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyMissionProgressSearchService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ILoyaltyMissionProgressService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<LoyaltyMissionProgressSearchCriteria, LoyaltyMissionProgressSearchResult, LoyaltyMissionProgress, LoyaltyMissionProgressEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ILoyaltyMissionProgressSearchService
{
    protected override IQueryable<LoyaltyMissionProgressEntity> BuildQuery(IRepository repository, LoyaltyMissionProgressSearchCriteria criteria)
    {
        var loyaltyRepository = (ILoyaltyRepository)repository;

        var query = loyaltyRepository.LoyaltyMissionProgresses;

        if (!criteria.MissionId.IsNullOrEmpty())
        {
            query = query.Where(x => x.MissionId == criteria.MissionId);
        }

        if (!criteria.MissionIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.MissionIds.Contains(x.MissionId));
        }

        if (!criteria.UserId.IsNullOrEmpty())
        {
            query = query.Where(x => x.UserId == criteria.UserId);
        }

        if (!criteria.Statuses.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.Statuses.Contains(x.Status));
        }

        if (!criteria.StoreId.IsNullOrEmpty())
        {
            var missionIds = loyaltyRepository.LoyaltyMissions
                .Where(m => m.StoreId == criteria.StoreId)
                .Select(m => m.Id);
            query = query.Where(x => missionIds.Contains(x.MissionId));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyMissionProgressSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(LoyaltyMissionProgressEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(LoyaltyMissionProgressEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
