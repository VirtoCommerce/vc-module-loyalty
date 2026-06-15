using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using static VirtoCommerce.Loyalty.Core.ModuleConstants;

namespace VirtoCommerce.Loyalty.Core.Extensions;

public static class StoreExtensions
{
    public static string GetLoyaltyCurrencyCode(this Store store, bool useFallbackCurrencyCode = true)
    {
        var currencyCode = store.Settings.GetValue<string>(Settings.General.LoyaltyCurrency);
        var fallbackCurrencyCode = useFallbackCurrencyCode ? FallbackLoyaltyCurrencyCode : null;
        return !currencyCode.IsNullOrEmpty() ? currencyCode : fallbackCurrencyCode;
    }
}
