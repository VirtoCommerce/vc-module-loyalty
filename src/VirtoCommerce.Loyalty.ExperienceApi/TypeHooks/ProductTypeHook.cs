using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CartModule.Core.Model;
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
using VirtoCommerce.XCart.Core.Schemas;
using VirtoCommerce.XCatalog.Core.Models;
using VirtoCommerce.XCatalog.Core.Schemas;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Loyalty.ExperienceApi.TypeHooks;

public class LineItemTypeHook : IGraphTypeHook
{
    public string TypeName { get; set; } = "LineItemType";

    public void BeforeTypeInitialized(IGraphType graphType)
    {
        if (graphType is not LineItemType lineItemType)
        {
            return;
        }

        var fieldAsync = FieldCreator.CreateFieldAsync<LineItem, MoneyType>(
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
              var loader = dataLoder.Context.GetOrAddBatchLoader<LineItem, Money>("cart_loyalty_points", async (lineItems) =>
              {
                  var currencyService = fieldContext.RequestServices.GetRequiredService<ICurrencyService>();
                  var productLoyaltyService = fieldContext.RequestServices.GetRequiredService<IProductLoyaltyProgramService>();
                  var factorSearchService = fieldContext.RequestServices.GetRequiredService<ILoyaltyProgramProductFactorSearchService>();
                  var storeService = fieldContext.RequestServices.GetRequiredService<IStoreService>();

                  var currencies = await currencyService.GetAllCurrenciesAsync();
                  var pointsCurrency = currencies.FirstOrDefault(x => x.Code.EqualsIgnoreCase(ModuleConstants.PointsCurrencyCode));
                  if (pointsCurrency == null)
                  {
                      return new Dictionary<LineItem, Money>();
                  }

                  var defaultFactor = await GetDefaultFactorAsync(fieldContext, storeService);
                  var loyaltyContext = CreateLoyaltyContext(fieldContext);
                  var loyaltyProgram = await productLoyaltyService.GetTopLoyaltyProgramAsync(loyaltyContext);
                  if (loyaltyProgram == null)
                  {
                      return CreatePointsResult(lineItems, pointsCurrency, defaultFactor);
                  }

                  // get factors for products
                  var productIds = lineItems.Select(x => x.ProductId).ToArray();
                  var factorCriteria = CreateLoyaltyFactorCriteria(productIds, loyaltyProgram);
                  var factors = await factorSearchService.SearchAllNoCloneAsync(factorCriteria);

                  return CreatePointsResult(lineItems, pointsCurrency, defaultFactor, factors);
              },
              keyComparer: AnonymousComparer.Create((LineItem x) => x.Id));

              return loader.LoadAsync(fieldContext.Source);

          });

        lineItemType.AddField(fieldAsync);
    }

    private static Dictionary<LineItem, Money> CreatePointsResult(IEnumerable<LineItem> lineItems, Currency pointsCurrency, decimal defaultFactor, IList<LoyaltyProgramProductFactor> factors = null)
    {
        return lineItems.ToDictionary(x => x, x =>
        {
            var price = x.ExtendedPrice;

            var factor = factors?.FirstOrDefault(f => f.ProductId == x.Id)?.Factor ?? defaultFactor;

            var pointsAmount = price * factor;
            var pointsMoney = new Money(pointsAmount, pointsCurrency);

            return pointsMoney;
        });
    }

    private static async Task<decimal> GetDefaultFactorAsync(IResolveFieldContext<LineItem> fieldContext, IStoreService storeService)
    {
        var storeId = fieldContext.GetArgumentOrValue<string>("storeId");
        var store = await storeService.GetByIdAsync(storeId);
        var defaultFactorValue = store.Settings.GetValue<decimal>(ModuleConstants.Settings.General.DefaultProductMultiplyFactor);
        return defaultFactorValue;
    }

    private static LoyaltyProgramProductFactorSearchCriteria CreateLoyaltyFactorCriteria(string[] productIds, LoyaltyProgram loyaltyProgram)
    {
        var factorCriteria = AbstractTypeFactory<LoyaltyProgramProductFactorSearchCriteria>.TryCreateInstance();
        factorCriteria.LoyaltyProgramId = loyaltyProgram.Id;
        factorCriteria.ProductIds = productIds;
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
              var loader = dataLoder.Context.GetOrAddBatchLoader<ExpProduct, Money>("product_loyalty_points", async (products) =>
              {
                  var currencyService = fieldContext.RequestServices.GetRequiredService<ICurrencyService>();
                  var productLoyaltyService = fieldContext.RequestServices.GetRequiredService<IProductLoyaltyProgramService>();
                  var factorSearchService = fieldContext.RequestServices.GetRequiredService<ILoyaltyProgramProductFactorSearchService>();
                  var storeService = fieldContext.RequestServices.GetRequiredService<IStoreService>();

                  var currencies = await currencyService.GetAllCurrenciesAsync();
                  var pointsCurrency = currencies.FirstOrDefault(x => x.Code.EqualsIgnoreCase(ModuleConstants.PointsCurrencyCode));
                  if (pointsCurrency == null)
                  {
                      return new Dictionary<ExpProduct, Money>();
                  }

                  var defaultFactor = await GetDefaultFactorAsync(fieldContext, storeService);
                  var loyaltyContext = CreateLoyaltyContext(fieldContext);
                  var loyaltyProgram = await productLoyaltyService.GetTopLoyaltyProgramAsync(loyaltyContext);
                  if (loyaltyProgram == null)
                  {
                      return CreatePointsResult(products, pointsCurrency, defaultFactor);
                  }

                  // get factors for products
                  var productIds = products.Select(x => x.Id).ToArray();
                  var factorCriteria = CreateLoyaltyFactorCriteria(productIds, loyaltyProgram);
                  var factors = await factorSearchService.SearchAllNoCloneAsync(factorCriteria);

                  return CreatePointsResult(products, pointsCurrency, defaultFactor, factors);
              },
              keyComparer: AnonymousComparer.Create((ExpProduct x) => x.Id));

              return loader.LoadAsync(fieldContext.Source);
          });

        productType.AddField(fieldAsync);
    }

    private static Dictionary<ExpProduct, Money> CreatePointsResult(IEnumerable<ExpProduct> products, Currency pointsCurrency, decimal defaultFactor, IList<LoyaltyProgramProductFactor> factors = null)
    {
        return products.ToDictionary(x => x, x =>
        {
            var price = x.AllPrices.FirstOrDefault();
            if (price == null)
            {
                return null;
            }

            var factor = factors?.FirstOrDefault(f => f.ProductId == x.Id)?.Factor ?? defaultFactor;

            var pointsAmount = price.ActualPrice.Amount * factor;
            var pointsMoney = new Money(pointsAmount, pointsCurrency);

            return pointsMoney;
        });
    }

    private static async Task<decimal> GetDefaultFactorAsync(IResolveFieldContext<ExpProduct> fieldContext, IStoreService storeService)
    {
        var storeId = fieldContext.GetArgumentOrValue<string>("storeId");
        var store = await storeService.GetByIdAsync(storeId);
        var defaultFactorValue = store.Settings.GetValue<decimal>(ModuleConstants.Settings.General.DefaultProductMultiplyFactor);
        return defaultFactorValue;
    }

    private static LoyaltyProgramProductFactorSearchCriteria CreateLoyaltyFactorCriteria(string[] productIds, LoyaltyProgram loyaltyProgram)
    {
        var factorCriteria = AbstractTypeFactory<LoyaltyProgramProductFactorSearchCriteria>.TryCreateInstance();
        factorCriteria.LoyaltyProgramId = loyaltyProgram.Id;
        factorCriteria.ProductIds = productIds;
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
