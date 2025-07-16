using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ITransactionLogSearchService : ISearchService<TransactionLogSearchCriteria, TransactionLogSearchResult, TransactionLog>;
