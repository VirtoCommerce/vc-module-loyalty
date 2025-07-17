using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.Loyalty.Data.Services;

public class TransactionLogSearchService(
        Func<ILoyaltyProgramRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        ITransactionLogService crudService,
        IOptions<CrudOptions> crudOptions)
    : SearchService<TransactionLogSearchCriteria, TransactionLogSearchResult, TransactionLog, TransactionLogEntity>(
        repositoryFactory,
        platformMemoryCache,
        crudService,
        crudOptions),
    ITransactionLogSearchService
{
    protected override IQueryable<TransactionLogEntity> BuildQuery(IRepository repository, TransactionLogSearchCriteria criteria)
    {
        var query = ((ILoyaltyProgramRepository)repository).Transactions;
        if (!criteria.SearchPhrase.IsNullOrEmpty())
        {
            // TODO: Comment?
            query = query.Where(x => x.Comment.Contains(criteria.SearchPhrase));
        }
        if (!criteria.LoyaltyProgramId.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.LoyaltyProgramId == x.LoyaltyProgramId);
        }
        if (!criteria.CustomerId.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.CustomerId == x.CustomerId);
        }
        if (criteria.OperationType.HasValue)
        {
            query = query.Where(x => criteria.OperationType == x.OperationType);
        }
        if (!criteria.ObjectIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ObjectIds.Contains(x.ObjectId));
        }
        if (!string.IsNullOrEmpty(criteria.ObjectType))
        {
            query = query.Where(x => criteria.ObjectType == x.ObjectType);
        }
        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(TransactionLogSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos = [new SortInfo { SortColumn = nameof(TransactionLogEntity.CreatedDate), SortDirection = SortDirection.Descending }];
        }

        return sortInfos;
    }
}
