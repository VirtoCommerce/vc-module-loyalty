using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyProgramProductFactorListItemSearchService(
    ILoyaltyProgramProductFactorSearchService searchService,
    IItemService itemService,
    ILoyaltyProgramService loyaltyProgramService)
    : ILoyaltyProgramProductFactorListItemSearchService
{
    public async Task<LoyaltyProgramProductFactorListItemSearchResult> SearchAsync(LoyaltyProgramProductFactorSearchCriteria criteria)
    {
        var searchResult = await searchService.SearchNoCloneAsync(criteria);

        var productByIds = new Dictionary<string, CatalogProduct>();
        if (!criteria.LoyaltyProgramId.IsNullOrEmpty())
        {
            var productIds = searchResult.Results.Select(x => x.ProductId).Distinct().ToList();
            var products = productIds.Count > 0
                ? await itemService.GetByIdsAsync(productIds, ItemResponseGroup.ItemInfo.ToString(), catalogId: null)
                : [];
            productByIds = products.ToDictionary(x => x.Id);
        }

        var programByIds = new Dictionary<string, LoyaltyProgram>();
        if (!criteria.ProductIds.IsNullOrEmpty())
        {
            var programIds = searchResult.Results.Select(x => x.LoyaltyProgramId).Distinct().ToList();
            var programs = programIds.Count > 0
                ? await loyaltyProgramService.GetNoCloneAsync(programIds)
                : [];
            programByIds = programs.ToDictionary(x => x.Id);
        }

        var listItems = searchResult.Results
            .Select(factor =>
            {
                var product = productByIds.GetValueOrDefault(factor.ProductId);
                var program = programByIds.GetValueOrDefault(factor.LoyaltyProgramId);

                var result = new LoyaltyProgramProductFactorListItem
                {
                    Id = factor.Id,
                    LoyaltyProgramId = factor.LoyaltyProgramId,
                    ProductId = factor.ProductId,
                    Factor = factor.Factor,
                    CreatedDate = factor.CreatedDate,
                    CreatedBy = factor.CreatedBy,
                    ModifiedDate = factor.ModifiedDate,
                    ModifiedBy = factor.ModifiedBy,
                    ProductCode = product?.Code,
                    ProductName = product?.Name,
                    ProgramName = program?.Name,
                };

                return result;
            })
            .ToList();

        return new LoyaltyProgramProductFactorListItemSearchResult
        {
            TotalCount = searchResult.TotalCount,
            Results = listItems,
        };
    }
}
