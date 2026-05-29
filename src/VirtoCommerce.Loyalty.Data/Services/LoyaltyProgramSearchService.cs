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

public class LoyaltyProgramSearchService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ILoyaltyProgramService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<LoyaltyProgramSearchCriteria, LoyaltyProgramSearchResult, LoyaltyProgram, LoyaltyProgramEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ILoyaltyProgramSearchService
{
    protected override IQueryable<LoyaltyProgramEntity> BuildQuery(IRepository repository, LoyaltyProgramSearchCriteria criteria)
    {
        var query = ((ILoyaltyRepository)repository).LoyaltyPrograms;

        if (criteria.OnlyActive)
        {
            var now = DateTime.UtcNow;
            query = query.Where(x => x.IsActive && (x.StartDate == null || x.StartDate <= now) && (x.EndDate == null || x.EndDate >= now));
        }

        if (!criteria.Keyword.IsNullOrEmpty())
        {
            query = query.Where(x => x.Name.Contains(criteria.Keyword));
        }

        if (!criteria.StoreIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.StoreIds.Contains(x.StoreId));
        }

        if (!criteria.ProgramType.IsNullOrEmpty())
        {
            query = query.Where(x => x.ProgramType == criteria.ProgramType);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyProgramSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(LoyaltyProgramEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(LoyaltyProgramEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
