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

public class LoyaltyProgramOperationLogSearchService(
    Func<ILoyaltyRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ILoyaltyProgramOperationLogService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<LoyaltyProgramOperationLogSearchCriteria, LoyaltyProgramOperationLogSearchResult, LoyaltyProgramOperationLog, LoyaltyProgramOperationLogEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ILoyaltyProgramOperationLogSearchService
{
    protected override IQueryable<LoyaltyProgramOperationLogEntity> BuildQuery(IRepository repository, LoyaltyProgramOperationLogSearchCriteria criteria)
    {
        var query = ((ILoyaltyRepository)repository).LoyaltyProgramOperationLogs;

        if (!criteria.UserId.IsNullOrEmpty())
        {
            query = query.Where(x => x.UserId == criteria.UserId);
        }

        if (!criteria.ObjectId.IsNullOrEmpty())
        {
            query = query.Where(x => x.ObjectId == criteria.ObjectId);
        }

        if (!criteria.ObjectType.IsNullOrEmpty())
        {
            query = query.Where(x => x.ObjectType == criteria.ObjectType);
        }

        if (!criteria.LoyaltyProgramId.IsNullOrEmpty())
        {
            query = query.Where(x => x.LoyaltyProgramId == criteria.LoyaltyProgramId);
        }

        if (!criteria.OperationType.IsNullOrEmpty())
        {
            query = query.Where(x => x.OperationType == criteria.OperationType);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(LoyaltyProgramOperationLogSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(LoyaltyProgramOperationLogEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(LoyaltyProgramOperationLogEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
