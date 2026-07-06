using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Permissions = VirtoCommerce.Loyalty.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.Loyalty.Web.Controllers.Api;

[Authorize]
[Route("api/loyalty-mission-goal-items")]
public class LoyaltyMissionGoalItemController(
    ILoyaltyMissionGoalItemService crudService,
    ILoyaltyMissionGoalItemSearchService searchService)
    : Controller
{
    [HttpPost("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyMissionGoalItemSearchResult>> Search([FromBody] LoyaltyMissionGoalItemSearchCriteria criteria)
    {
        var result = await searchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Permissions.Create)]
    public Task<ActionResult<LoyaltyMissionGoalItem>> Create([FromBody] LoyaltyMissionGoalItem model)
    {
        model.Id = null;
        return Update(model);
    }

    [HttpPut]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<LoyaltyMissionGoalItem>> Update([FromBody] LoyaltyMissionGoalItem model)
    {
        if (model.Quantity < 0)
        {
            return BadRequest(InvalidQuantityError(model));
        }

        await crudService.SaveChangesAsync([model]);
        return Ok(model);
    }

    [HttpPut("items")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult> UpdateItems([FromBody] IList<LoyaltyMissionGoalItem> models)
    {
        var invalid = models.FirstOrDefault(x => x.Quantity < 0);
        if (invalid != null)
        {
            return BadRequest(InvalidQuantityError(invalid));
        }

        await crudService.SaveChangesAsync(models);
        return NoContent();
    }

    [HttpGet("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyMissionGoalItem>> Get([FromRoute] string id, [FromQuery] string responseGroup = null)
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

    private static string InvalidQuantityError(LoyaltyMissionGoalItem model)
    {
        return $"Goal item ID:{model.Id}, Mission ID: {model.MissionId}, Product ID: {model.ProductId} quantity can't be negative. Quantity: {model.Quantity}";
    }
}
