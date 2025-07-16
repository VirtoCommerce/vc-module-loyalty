using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Models;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyProgramSearchService : SearchService<LoyaltyProgramSearchCriteria, LoyaltyProgramSearchResult, LoyaltyProgram, LoyaltyProgramEntity>,
    ILoyaltyProgramSearchService
{
    public LoyaltyProgramSearchService(
        Func<ILoyaltyProgramRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        ILoyaltyProgramService crudService,
        IOptions<CrudOptions> crudOptions)
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<LoyaltyProgramEntity> BuildQuery(IRepository repository, LoyaltyProgramSearchCriteria criteria)
    {
        var query = ((ILoyaltyProgramRepository)repository).LoyaltyPrograms;

        if (!criteria.SearchPhrase.IsNullOrEmpty())
        {
            query = query.Where(x => x.Name.Contains(criteria.SearchPhrase));
        }

        if (!criteria.StoreIds.IsNullOrEmpty())
        {
            query = query.Where(x => !x.Stores.Any() || x.Stores.Any(s => criteria.StoreIds.Contains(s.StoreId)));
        }

        if (criteria.IsActive.HasValue)
        {
            var utcNow = DateTime.UtcNow;
            query = query.Where(x => x.IsActive == criteria.IsActive && (x.StartDate == null || utcNow >= x.StartDate) && (x.EndDate == null || x.EndDate >= utcNow));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyProgramSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos = [new SortInfo { SortColumn = nameof(LoyaltyProgram.Priority), SortDirection = SortDirection.Descending }];
        }

        return sortInfos;
    }
}
