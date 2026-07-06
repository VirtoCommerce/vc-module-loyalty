using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Models.Missions;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Permissions = VirtoCommerce.Loyalty.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.Loyalty.Web.Controllers.Api;

[Authorize]
[Route("api/loyalty-missions")]
public class LoyaltyMissionController(
    ILoyaltyMissionService crudService,
    ILoyaltyMissionSearchService searchService)
    : Controller
{
    [HttpPost("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyMissionSearchResult>> Search([FromBody] LoyaltyMissionSearchCriteria criteria)
    {
        var result = await searchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Permissions.Create)]
    public Task<ActionResult<LoyaltyMission>> Create([FromBody] LoyaltyMission model)
    {
        model.Id = null;
        return Update(model);
    }

    [HttpPut]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<LoyaltyMission>> Update([FromBody] LoyaltyMission model)
    {
        await crudService.SaveChangesAsync([model]);
        return Ok(model);
    }

    [HttpGet("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyMission>> Get([FromRoute] string id, [FromQuery] string responseGroup = null)
    {
        var model = await crudService.GetNoCloneAsync(id, responseGroup);

        model?.DynamicExpression?.MergeFromPrototype(AbstractTypeFactory<LoyaltyMissionConditionAndRewardTreePrototype>.TryCreateInstance());

        return Ok(model);
    }

    [HttpGet("new")]
    [Authorize(Permissions.Create)]
    public ActionResult<LoyaltyMission> GetNewLoyaltyMission()
    {
        var result = AbstractTypeFactory<LoyaltyMission>.TryCreateInstance();

        result.DynamicExpression.MergeFromPrototype(AbstractTypeFactory<LoyaltyMissionConditionAndRewardTreePrototype>.TryCreateInstance());
        result.Status = ModuleConstants.MissionStatuses.Draft;
        result.Periodicity = ModuleConstants.MissionPeriodicities.None;

        return Ok(result);
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
