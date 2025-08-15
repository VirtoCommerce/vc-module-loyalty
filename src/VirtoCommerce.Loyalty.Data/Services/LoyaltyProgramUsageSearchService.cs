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

public class LoyaltyProgramUsageSearchService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ILoyaltyProgramUsageService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<LoyaltyProgramUsageSearchCriteria, LoyaltyProgramUsageSearchResult, LoyaltyProgramUsage, LoyaltyProgramUsageEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ILoyaltyProgramUsageSearchService
{
    protected override IQueryable<LoyaltyProgramUsageEntity> BuildQuery(IRepository repository, LoyaltyProgramUsageSearchCriteria criteria)
    {
        var query = ((ILoyaltyRepository)repository).LoyaltyProgramUsages;

        if (!criteria.UserId.IsNullOrEmpty())
        {
            query = query.Where(x => x.UserId == criteria.UserId);
        }

        if (!criteria.OrderId.IsNullOrEmpty())
        {
            query = query.Where(x => x.OrderId == criteria.OrderId);
        }

        if (!criteria.LoyaltyProgramId.IsNullOrEmpty())
        {
            query = query.Where(x => x.LoyaltyProgramId == criteria.LoyaltyProgramId);
        }

        if (!criteria.UsageType.IsNullOrEmpty())
        {
            query = query.Where(x => x.UsageType == criteria.UsageType);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyProgramUsageSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(LoyaltyProgramUsageEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(LoyaltyProgramUsageEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
