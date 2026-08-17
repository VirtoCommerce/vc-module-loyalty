using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyBalanceOperationLogSearchService : ISearchService<LoyaltyBalanceOperationLogSearchCriteria, LoyaltyBalanceOperationLogSearchResult, LoyaltyBalanceOperationLog>;
