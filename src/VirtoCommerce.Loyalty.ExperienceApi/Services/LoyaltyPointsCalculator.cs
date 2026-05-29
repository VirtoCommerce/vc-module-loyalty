using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.Loyalty.ExperienceApi.Services;

public class LoyaltyPointsCalculator(
    ICurrencyService currencyService,
    IStoreService storeService,
    IProductLoyaltyProgramService productLoyaltyService,
    ILoyaltyProgramProductFactorSearchService factorSearchService)
    : ILoyaltyPointsCalculator
{
    public async Task<LoyaltyPointsContext> ResolveAsync(string storeId, string userId, string language, string currencyCode, IList<string> productIds)
    {
        var store = await storeService.GetByIdAsync(storeId);
        var loyaltyEnabled = store.Settings.GetValue<bool>(ModuleConstants.Settings.General.Enable);
        if (!loyaltyEnabled)
        {
            return new LoyaltyPointsContext();
        }

        var currencies = await currencyService.GetAllCurrenciesAsync();
        var pointsCurrencyCode = GetLoyaltyCurrencyCode(store);
        var pointsCurrency = currencies.FirstOrDefault(x => x.Code.EqualsIgnoreCase(pointsCurrencyCode));

        var defaultFactor = GetDefaultFactor(store);

        var factorByProductId = new Dictionary<string, decimal>();

        var loyaltyContext = CreateLoyaltyContext(storeId, userId, language, currencyCode);
        var loyaltyProgram = await productLoyaltyService.GetTopLoyaltyProgramAsync(loyaltyContext);
        if (loyaltyProgram != null && productIds.Count > 0)
        {
            var factorCriteria = AbstractTypeFactory<LoyaltyProgramProductFactorSearchCriteria>.TryCreateInstance();
            factorCriteria.LoyaltyProgramId = loyaltyProgram.Id;
            factorCriteria.ProductIds = [.. productIds];

            var factors = await factorSearchService.SearchAllNoCloneAsync(factorCriteria);
            foreach (var factor in factors)
            {
                factorByProductId[factor.ProductId] = factor.Factor;
            }
        }

        return new LoyaltyPointsContext
        {
            PointsCurrency = pointsCurrency,
            DefaultFactor = defaultFactor,
            FactorByProductId = factorByProductId,
        };
    }

    private static decimal GetDefaultFactor(Store store)
    {
        return store.Settings.GetValue<decimal>(ModuleConstants.Settings.General.DefaultProductMultiplyFactor);
    }

    private static string GetLoyaltyCurrencyCode(Store store)
    {
        var currencyCode = store.Settings.GetValue<string>(ModuleConstants.Settings.General.LoyaltyCurrency);
        return !currencyCode.IsNullOrEmpty() ? currencyCode : ModuleConstants.FallbackLoyaltyCurrencyCode;
    }

    private static LoyaltyProgramEvaluationContext CreateLoyaltyContext(string storeId, string userId, string language, string currencyCode)
    {
        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.UserId = userId;
        context.StoreId = storeId;
        context.Language = language;
        context.CurrencyCode = currencyCode;
        context.ProgramType = ModuleConstants.LoyaltyPrograms.ProductProgramType;
        return context;
    }
}
