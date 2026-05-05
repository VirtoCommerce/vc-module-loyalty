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

public class LoyaltyProgramProductFactorSearchService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ILoyaltyProgramProductFactorService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<LoyaltyProgramProductFactorSearchCriteria, LoyaltyProgramProductFactorSearchResult, LoyaltyProgramProductFactor, LoyaltyProgramProductFactorEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ILoyaltyProgramProductFactorSearchService
{
    protected override IQueryable<LoyaltyProgramProductFactorEntity> BuildQuery(IRepository repository, LoyaltyProgramProductFactorSearchCriteria criteria)
    {
        var query = ((ILoyaltyRepository)repository).LoyaltyProgramProductFactors;

        if (!criteria.LoyaltyProgramId.IsNullOrEmpty())
        {
            query = query.Where(x => x.LoyaltyProgramId == criteria.LoyaltyProgramId);
        }

        if (!criteria.ProductIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ProductIds.Contains(x.ProductId));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyProgramProductFactorSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(LoyaltyProgramProductFactorEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(LoyaltyProgramProductFactorEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
