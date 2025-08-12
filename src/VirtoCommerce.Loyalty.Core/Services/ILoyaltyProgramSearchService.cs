using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyProgramSearchService : ISearchService<LoyaltyProgramSearchCriteria, LoyaltyProgramSearchResult, LoyaltyProgram>;
