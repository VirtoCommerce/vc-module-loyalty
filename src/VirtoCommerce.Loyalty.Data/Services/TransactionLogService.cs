using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Events;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.Loyalty.Data.Services;

public class TransactionLogService(
        Func<ILoyaltyProgramRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
    : CrudService<TransactionLog, TransactionLogEntity, TransactionLogChangingEvent, TransactionLogChangedEvent>(
        repositoryFactory,
        platformMemoryCache,
        eventPublisher),
    ITransactionLogService
{
    protected override Task<IList<TransactionLogEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ILoyaltyProgramRepository)repository).GetTransactionLogsByIdsAsync(ids);
    }

    public async Task<decimal> GetPointsByCustomerIdAsync(string customerId, ITransactionLogSearchService transactionSearchService)
    {
        const decimal ZeroPoints = 0;
        var transactionCriteria = AbstractTypeFactory<TransactionLogSearchCriteria>.TryCreateInstance();
        transactionCriteria.CustomerId = customerId;
        var transactions = (await transactionSearchService.SearchAsync(transactionCriteria)).Results;
        if (!transactions.Any())
        {
            return ZeroPoints;
        }

        var latestTransaction = transactions.OrderByDescending(t => t.CreatedDate).FirstOrDefault();
        return (latestTransaction?.Balance) ?? ZeroPoints;
    }
}
