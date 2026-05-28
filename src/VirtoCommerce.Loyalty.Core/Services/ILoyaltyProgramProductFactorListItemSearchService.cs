using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyProgramProductFactorListItemSearchService
{
    Task<LoyaltyProgramProductFactorListItemSearchResult> SearchAsync(LoyaltyProgramProductFactorSearchCriteria criteria);
}
