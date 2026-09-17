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

public class LoyaltyBalanceOperationLogSearchService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ILoyaltyBalanceOperationLogService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<LoyaltyBalanceOperationLogSearchCriteria, LoyaltyBalanceOperationLogSearchResult, LoyaltyBalanceOperationLog, LoyaltyBalanceOperationLogEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ILoyaltyBalanceOperationLogSearchService
{
    protected override IQueryable<LoyaltyBalanceOperationLogEntity> BuildQuery(IRepository repository, LoyaltyBalanceOperationLogSearchCriteria criteria)
    {
        var query = ((ILoyaltyRepository)repository).LoyaltyBalanceOperationLogs;

        // build "UserId or OrganizationId" predicate
        query = BuildOwnerQuery(criteria, query);

        if (!criteria.ObjectId.IsNullOrEmpty())
        {
            query = query.Where(x => x.ObjectId == criteria.ObjectId);
        }

        if (!criteria.ObjectType.IsNullOrEmpty())
        {
            query = query.Where(x => x.ObjectType == criteria.ObjectType);
        }

        if (!criteria.SourceType.IsNullOrEmpty())
        {
            query = query.Where(x => x.SourceType == criteria.SourceType);
        }

        if (!criteria.SourceId.IsNullOrEmpty())
        {
            query = query.Where(x => x.SourceId == criteria.SourceId);
        }

        if (!criteria.OperationType.IsNullOrEmpty())
        {
            query = query.Where(x => x.OperationType == criteria.OperationType);
        }

        return query;
    }

    protected virtual IQueryable<LoyaltyBalanceOperationLogEntity> BuildOwnerQuery(LoyaltyBalanceOperationLogSearchCriteria criteria, IQueryable<LoyaltyBalanceOperationLogEntity> query)
    {
        if (criteria.UserId.IsNullOrEmpty() && criteria.OrganizationId.IsNullOrEmpty())
        {
            return query;
        }

        var predicate = PredicateBuilder.False<LoyaltyBalanceOperationLogEntity>();

        if (!criteria.UserId.IsNullOrEmpty())
        {
            predicate = predicate.Or(x => x.UserId == criteria.UserId && x.OrganizationId == null);
        }

        if (!criteria.OrganizationId.IsNullOrEmpty())
        {
            predicate = predicate.Or(x => x.OrganizationId == criteria.OrganizationId);
        }

        query = query.Where(predicate);

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyBalanceOperationLogSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(LoyaltyBalanceOperationLogEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(LoyaltyBalanceOperationLogEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
