using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Permissions = VirtoCommerce.Loyalty.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.Loyalty.Web.Controllers.Api;

[Authorize]
[Route("api/loyalty-program-product-factors")]
public class LoyaltyProgramProductFactorController(
    ILoyaltyProgramProductFactorService crudService,
    ILoyaltyProgramProductFactorSearchService searchService,
    IItemService itemService)
    : Controller
{
    [HttpPost("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgramProductFactorListItemSearchResult>> Search([FromBody] LoyaltyProgramProductFactorSearchCriteria criteria)
    {
        var searchResult = await searchService.SearchNoCloneAsync(criteria);

        var productIds = searchResult.Results.Select(x => x.ProductId).Distinct().ToList();
        var products = productIds.Count > 0
            ? await itemService.GetByIdsAsync(productIds, ItemResponseGroup.ItemInfo.ToString(), catalogId: null)
            : [];
        var productById = products.ToDictionary(x => x.Id);

        var listItems = searchResult.Results
            .Select(factor => new LoyaltyProgramProductFactorListItem
            {
                Id = factor.Id,
                LoyaltyProgramId = factor.LoyaltyProgramId,
                ProductId = factor.ProductId,
                Factor = factor.Factor,
                CreatedDate = factor.CreatedDate,
                CreatedBy = factor.CreatedBy,
                ModifiedDate = factor.ModifiedDate,
                ModifiedBy = factor.ModifiedBy,
                Product = productById.GetValueOrDefault(factor.ProductId),
            })
            .ToList();

        var result = new LoyaltyProgramProductFactorListItemSearchResult
        {
            TotalCount = searchResult.TotalCount,
            Results = listItems,
        };

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Permissions.Create)]
    public Task<ActionResult<LoyaltyProgramProductFactor>> Create([FromBody] LoyaltyProgramProductFactor model)
    {
        model.Id = null;
        return Update(model);
    }

    [HttpPut]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<LoyaltyProgramProductFactor>> Update([FromBody] LoyaltyProgramProductFactor model)
    {
        await crudService.SaveChangesAsync([model]);
        return Ok(model);
    }

    [HttpPut("factors")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult> UpdateFactors([FromBody] IList<LoyaltyProgramProductFactor> models)
    {
        await crudService.SaveChangesAsync(models);
        return NoContent();
    }

    [HttpGet("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgramProductFactor>> Get([FromRoute] string id, [FromQuery] string responseGroup = null)
    {
        var model = await crudService.GetNoCloneAsync(id, responseGroup);
        return Ok(model);
    }

    [HttpDelete]
    [Authorize(Permissions.Delete)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        await crudService.DeleteAsync(ids);
        return NoContent();
    }
}
