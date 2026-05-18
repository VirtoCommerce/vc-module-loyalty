using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;

namespace VirtoCommerce.Loyalty.Web.Controllers.Api;

[Authorize]
[Route("api/loyalty-setting")]
public class LoyaltySettingController : Controller
{
    private readonly ILoyaltySettingService _loyaltySettingService;

    public LoyaltySettingController(ILoyaltySettingService loyaltySettingsService)
    {
        _loyaltySettingService = loyaltySettingsService;
    }

    [HttpGet("store/{storeId}")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<LoyaltyStoreSetting>> GetByStoreId([FromRoute] string storeId)
    {
        var result = await _loyaltySettingService.GetByStoreIdAsync(storeId);
        return Ok(result);
    }

    [HttpPut]
    [Route("")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [Authorize(ModuleConstants.Security.Permissions.Update)]
    public async Task<ActionResult> Update([FromBody] LoyaltyStoreSetting model)
    {
        await _loyaltySettingService.UpdateAsync(model);
        return NoContent();
    }
}
