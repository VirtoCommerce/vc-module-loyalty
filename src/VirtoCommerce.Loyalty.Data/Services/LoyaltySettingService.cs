using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.Loyalty.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltySettingService : ILoyaltySettingService
{
    private readonly IStoreService _storeService;

    public LoyaltySettingService(IStoreService storeService)
    {
        _storeService = storeService;
    }

    public async Task<LoyaltyStoreSetting> GetByStoreIdAsync(string storeId)
    {
        var store = await _storeService.GetNoCloneAsync(storeId);

        if (store == null)
        {
            return null;
        }

        var loyaltyStoreSettings = AbstractTypeFactory<LoyaltyStoreSetting>.TryCreateInstance();

        loyaltyStoreSettings.StoreId = store.Id;

        loyaltyStoreSettings.LoyaltyEnabled = store.Settings.GetValue<bool>(Enable);
        loyaltyStoreSettings.LoyaltyMode = store.Settings.GetValue<string>(LoyaltyMode);
        loyaltyStoreSettings.LoyaltyCurrency = store.Settings.GetValue<string>(LoyaltyCurrency);

        return loyaltyStoreSettings;
    }

    public async Task UpdateAsync(LoyaltyStoreSetting loyaltyStoreSettings)
    {
        var store = await _storeService.GetByIdAsync(loyaltyStoreSettings.StoreId);

        if (store == null)
        {
            return;
        }

        var loyaltyEnabledSetting = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(Enable.Name));
        if (loyaltyEnabledSetting == null)
        {
            loyaltyEnabledSetting = new ObjectSettingEntry(Enable);
            store.Settings.Add(loyaltyEnabledSetting);
        }
        loyaltyEnabledSetting.Value = loyaltyStoreSettings.LoyaltyEnabled;

        var loyaltyModeSetting = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(LoyaltyMode.Name));
        if (loyaltyModeSetting == null)
        {
            loyaltyModeSetting = new ObjectSettingEntry(LoyaltyMode);
            store.Settings.Add(loyaltyModeSetting);
        }
        loyaltyModeSetting.Value = loyaltyStoreSettings.LoyaltyMode ?? string.Empty;

        var loyaltyCurrencySetting = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(LoyaltyCurrency.Name));
        if (loyaltyCurrencySetting == null)
        {
            loyaltyCurrencySetting = new ObjectSettingEntry(LoyaltyCurrency);
            store.Settings.Add(loyaltyCurrencySetting);
        }
        loyaltyCurrencySetting.Value = loyaltyStoreSettings.LoyaltyCurrency ?? string.Empty;

        await _storeService.SaveChangesAsync([store]);
    }
}
