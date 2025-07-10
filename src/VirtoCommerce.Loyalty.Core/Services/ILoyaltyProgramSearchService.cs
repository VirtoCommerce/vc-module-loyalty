using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Models;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.LoyaltyProgramSearchService.Core.Services;

public interface ILoyaltyProgramSearchService : ISearchService<LoyaltyProgramSearchCriteria, LoyaltyProgramSearchResult, LoyaltyProgram>;
