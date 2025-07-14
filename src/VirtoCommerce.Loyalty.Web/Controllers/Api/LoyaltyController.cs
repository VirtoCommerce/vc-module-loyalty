using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Models;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.GenericCrud;
using Permissions = VirtoCommerce.Loyalty.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.Loyalty.Web.Controllers.Api;

[Authorize]
[Route("api/loyalty")]
public class LoyaltyController : Controller
{
    private readonly ILoyaltyProgramService _loyaltyService;
    private readonly ILoyaltyProgramSearchService _searchService;

    public LoyaltyController(ILoyaltyProgramService loyaltyService, ILoyaltyProgramSearchService searchService)
    {
        _loyaltyService = loyaltyService;
        _searchService = searchService;
    }

    [HttpPost("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgramSearchResult>> Search([FromBody] LoyaltyProgramSearchCriteria criteria)
    {
        var result = await _searchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpPost]
    [Route("")]
    [Authorize(Permissions.Create)]
    public async Task<ActionResult<LoyaltyProgram>> Create([FromBody] LoyaltyProgram loyaltyProgram)
    {
        loyaltyProgram.Id = null;
        loyaltyProgram.Conditions = "";
        await _loyaltyService.SaveChangesAsync([loyaltyProgram]);
        return Ok(loyaltyProgram);
    }

    [HttpGet]
    [Route("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgram>> Get([FromRoute] string id)
    {
        var result = await _loyaltyService.GetNoCloneAsync(id);
        return Ok(result);
    }

    [HttpPut]
    [Route("")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<LoyaltyProgram>> Update([FromBody] LoyaltyProgram loyaltyProgram)
    {
        await _loyaltyService.SaveChangesAsync([loyaltyProgram]);
        return Ok(loyaltyProgram);
    }

    [HttpDelete]
    [Route("")]
    [Authorize(Permissions.Delete)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        await _loyaltyService.DeleteAsync(ids);
        return NoContent();
    }

    [HttpGet]
    [Route("new")]
    [Authorize(Permissions.Create)]
    public ActionResult<LoyaltyProgram> GetNewDynamicPromotion()
    {
        var retVal = AbstractTypeFactory<LoyaltyProgram>.TryCreateInstance();
        return Ok(retVal);
    }
}
