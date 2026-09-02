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

public class LoyaltyMissionGoalItemSearchService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ILoyaltyMissionGoalItemService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<LoyaltyMissionGoalItemSearchCriteria, LoyaltyMissionGoalItemSearchResult, LoyaltyMissionGoalItem, LoyaltyMissionGoalItemEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ILoyaltyMissionGoalItemSearchService
{
    protected override IQueryable<LoyaltyMissionGoalItemEntity> BuildQuery(IRepository repository, LoyaltyMissionGoalItemSearchCriteria criteria)
    {
        var query = ((ILoyaltyRepository)repository).LoyaltyMissionGoalItems;

        if (!criteria.MissionId.IsNullOrEmpty())
        {
            query = query.Where(x => x.MissionId == criteria.MissionId);
        }

        if (!criteria.MissionIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.MissionIds.Contains(x.MissionId));
        }

        if (!criteria.ProductIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ProductIds.Contains(x.ProductId));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyMissionGoalItemSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(LoyaltyMissionGoalItemEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(LoyaltyMissionGoalItemEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
