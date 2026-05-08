using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Schemas;
using VirtoCommerce.XCatalog.Core.Models;
using VirtoCommerce.XCatalog.Core.Schemas;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Loyalty.ExperienceApi.TypeHooks;

public class ProductTypeHook : IGraphTypeHook
{
    public string TypeName { get; set; } = "Product";

    public void BeforeTypeInitialized(IGraphType graphType)
    {
        if (graphType is not ProductType productType)
        {
            return;
        }

        var fieldAsync = FieldCreator.CreateFieldAsync<ExpProduct, MoneyType>(
          "loyaltyPoints",
          "Get points amount",
          resolve: async fieldContext =>
          {
              if (fieldContext.Source == null)
              {
                  return null;
              }

              if (fieldContext.User.GetCurrentUserId() == AnonymousUser.UserName)
              {
                  return null;
              }

              var dataLoder = fieldContext.RequestServices.GetRequiredService<IDataLoaderContextAccessor>();
              var loader = dataLoder.Context.GetOrAddBatchLoader<ExpProduct, Money>("loyalty_points", async (products) =>
              {
                  var currencyService = fieldContext.RequestServices.GetRequiredService<ICurrencyService>();
                  var productLoyaltyService = fieldContext.RequestServices.GetRequiredService<IProductLoyaltyProgramService>();
                  var factorSearchService = fieldContext.RequestServices.GetRequiredService<ILoyaltyProgramProductFactorSearchService>();
                  var storeService = fieldContext.RequestServices.GetRequiredService<IStoreService>();

                  var result = new Dictionary<ExpProduct, Money>();

                  var currencies = await currencyService.GetAllCurrenciesAsync();
                  var pointsCurrency = currencies.FirstOrDefault(x => x.Code.EqualsIgnoreCase("XPT"));
                  if (pointsCurrency == null)
                  {
                      return result;
                  }

                  var loyaltyContext = CreateLoyaltyContext(fieldContext);
                  var loyaltyProgram = await productLoyaltyService.GetTopLoyaltyProgramAsync(loyaltyContext);
                  if (loyaltyProgram == null)
                  {
                      // apply default factor to all products if no loyalty program found
                      var factor = await GetDefaultFactorAsync(fieldContext, storeService);

                      foreach (var product in products)
                      {
                          result.TryAdd(product, null);

                          var price = product.AllPrices.FirstOrDefault();
                          if (price == null)
                          {
                              continue;
                          }

                          var pointsAmount = price.ActualPrice.Amount * factor;
                          var pointsMoney = new Money(pointsAmount, pointsCurrency);

                          result[product] = pointsMoney;
                      }

                      return result;
                  }

                  // get factors for products
                  var factorCriteria = CreateLoyaltyFactorCriteria(products, loyaltyProgram);
                  var factors = await factorSearchService.SearchAllNoCloneAsync(factorCriteria);

                  decimal? defaultFactor = null;

                  foreach (var product in products)
                  {
                      result.TryAdd(product, null);

                      var price = product.AllPrices.FirstOrDefault();
                      if (price == null)
                      {
                          continue;
                      }

                      var productFactor = factors.FirstOrDefault(x => x.ProductId == product.Id);
                      if (productFactor == null)
                      {
                          defaultFactor ??= await GetDefaultFactorAsync(fieldContext, storeService);
                          productFactor = new LoyaltyProgramProductFactor { Factor = defaultFactor.Value };
                      }

                      var pointsAmount = price.ActualPrice.Amount * productFactor.Factor;
                      var pointsMoney = new Money(pointsAmount, pointsCurrency);

                      result[product] = pointsMoney;
                  }

                  return result;

              }, keyComparer: AnonymousComparer.Create((ExpProduct x) => x.Id));

              return loader.LoadAsync(fieldContext.Source);
          });

        productType.AddField(fieldAsync);
    }

    private async Task<decimal> GetDefaultFactorAsync(IResolveFieldContext<ExpProduct> fieldContext, IStoreService storeService)
    {
        var storeId = fieldContext.GetArgumentOrValue<string>("storeId");
        var store = await storeService.GetByIdAsync(storeId);
        var defaultFactorValue = store.Settings.GetValue<decimal>(ModuleConstants.Settings.General.DefaultProductMultiplyFactor);
        return defaultFactorValue;
    }

    private static LoyaltyProgramProductFactorSearchCriteria CreateLoyaltyFactorCriteria(IEnumerable<ExpProduct> products, LoyaltyProgram loyaltyProgram)
    {
        var factorCriteria = AbstractTypeFactory<LoyaltyProgramProductFactorSearchCriteria>.TryCreateInstance();
        factorCriteria.LoyaltyProgramId = loyaltyProgram.Id;
        factorCriteria.ProductIds = products.Select(x => x.Id).ToArray();
        return factorCriteria;
    }

    private static LoyaltyProgramEvaluationContext CreateLoyaltyContext(IResolveFieldContext fieldContext)
    {
        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();

        var userId = fieldContext.User.GetCurrentUserId();
        var storeId = fieldContext.GetArgumentOrValue<string>("storeId");
        var cultureName = fieldContext.GetArgumentOrValue<string>("cultureName");
        var currencyCode = fieldContext.GetArgumentOrValue<string>("currencyCode");

        context.UserId = userId;
        context.StoreId = storeId;
        context.Language = cultureName;
        context.CurrencyCode = currencyCode;
        context.ProgramType = ModuleConstants.LoyaltyPrograms.ProductProgramType;

        return context;
    }
}
