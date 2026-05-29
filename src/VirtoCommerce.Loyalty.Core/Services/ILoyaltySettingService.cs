using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltySettingService
{
    Task<LoyaltyStoreSetting> GetByStoreIdAsync(string storeId);
    Task UpdateAsync(LoyaltyStoreSetting loyaltyStoreSettings);
}
