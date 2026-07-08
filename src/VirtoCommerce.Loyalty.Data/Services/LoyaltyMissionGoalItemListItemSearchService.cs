using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyMissionGoalItemListItemSearchService(
    ILoyaltyMissionGoalItemSearchService searchService,
    IItemService itemService)
    : ILoyaltyMissionGoalItemListItemSearchService
{
    public async Task<LoyaltyMissionGoalItemListItemSearchResult> SearchAsync(LoyaltyMissionGoalItemSearchCriteria criteria)
    {
        var searchResult = await searchService.SearchNoCloneAsync(criteria);

        var productByIds = new Dictionary<string, CatalogProduct>();
        var productIds = searchResult.Results.Select(x => x.ProductId).Distinct().ToList();
        if (productIds.Count > 0)
        {
            var products = await itemService.GetByIdsAsync(productIds, ItemResponseGroup.ItemInfo.ToString(), catalogId: null);
            productByIds = products.ToDictionary(x => x.Id);
        }

        var listItems = searchResult.Results
            .Select(goalItem =>
            {
                var product = productByIds.GetValueOrDefault(goalItem.ProductId);

                return new LoyaltyMissionGoalItemListItem
                {
                    Id = goalItem.Id,
                    MissionId = goalItem.MissionId,
                    ProductId = goalItem.ProductId,
                    Quantity = goalItem.Quantity,
                    CreatedDate = goalItem.CreatedDate,
                    CreatedBy = goalItem.CreatedBy,
                    ModifiedDate = goalItem.ModifiedDate,
                    ModifiedBy = goalItem.ModifiedBy,
                    ProductCode = product?.Code,
                    ProductName = product?.Name,
                };
            })
            .ToList();

        return new LoyaltyMissionGoalItemListItemSearchResult
        {
            TotalCount = searchResult.TotalCount,
            Results = listItems,
        };
    }
}
