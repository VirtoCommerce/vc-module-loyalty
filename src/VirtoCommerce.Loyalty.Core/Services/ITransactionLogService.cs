using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ITransactionLogService : ICrudService<TransactionLog>
{
    Task<decimal> GetPointsByCustomerIdAsync(string customerId, ITransactionLogSearchService transactionSearchService);
}
