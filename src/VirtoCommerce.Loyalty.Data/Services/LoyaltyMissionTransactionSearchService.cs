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

public class LoyaltyMissionTransactionSearchService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ILoyaltyMissionTransactionService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<LoyaltyMissionTransactionSearchCriteria, LoyaltyMissionTransactionSearchResult, LoyaltyMissionTransaction, LoyaltyMissionTransactionEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ILoyaltyMissionTransactionSearchService
{
    protected override IQueryable<LoyaltyMissionTransactionEntity> BuildQuery(IRepository repository, LoyaltyMissionTransactionSearchCriteria criteria)
    {
        var query = ((ILoyaltyRepository)repository).LoyaltyMissionTransactions;

        if (!criteria.MissionId.IsNullOrEmpty())
        {
            query = query.Where(x => x.MissionId == criteria.MissionId);
        }

        if (!criteria.MissionProgressId.IsNullOrEmpty())
        {
            query = query.Where(x => x.MissionProgressId == criteria.MissionProgressId);
        }

        if (!criteria.UserId.IsNullOrEmpty())
        {
            query = query.Where(x => x.UserId == criteria.UserId);
        }

        if (!criteria.ObjectId.IsNullOrEmpty())
        {
            query = query.Where(x => x.ObjectId == criteria.ObjectId);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyMissionTransactionSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(LoyaltyMissionTransactionEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(LoyaltyMissionTransactionEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
