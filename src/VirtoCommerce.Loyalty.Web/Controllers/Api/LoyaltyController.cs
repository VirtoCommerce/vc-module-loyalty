using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Permissions = VirtoCommerce.Loyalty.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.Loyalty.Web.Controllers.Api;

[Authorize]
[Route("api/loyalty")]
public class LoyaltyController : Controller
{
    private readonly ILoyaltyProgramService _loyaltyService;

    public LoyaltyController(ILoyaltyProgramService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    [HttpPost]
    [Route("")]
    [Authorize(Permissions.Create)]
    public async Task<ActionResult<LoyaltyProgram>> Create([FromBody] LoyaltyProgram loyaltyProgram)
    {
        loyaltyProgram.Id = null;
        await _loyaltyService.SaveChangesAsync([loyaltyProgram]);
        return Ok(loyaltyProgram);
    }

    [HttpGet]
    [Route("")]
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
}
