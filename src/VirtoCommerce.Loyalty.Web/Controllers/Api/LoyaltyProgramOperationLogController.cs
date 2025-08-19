using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Permissions = VirtoCommerce.Loyalty.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.Loyalty.Web.Controllers.Api;

[Authorize]
[Route("api/loyalty-program-operation-log")]
public class LoyaltyProgramOperationLogController(
    ILoyaltyProgramOperationLogSearchService searchService,
    ILoyaltyLogicService loyaltyLogicService)
    : Controller
{
    [HttpPost("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgramOperationLogSearchResult>> Search([FromBody] LoyaltyProgramOperationLogSearchCriteria criteria)
    {
        var result = await searchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpGet("balance/{userId}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgramOperationLog>> GetBalance([FromRoute] string userId)
    {
        var balance = await loyaltyLogicService.GetUserBalanceAsync(userId);

        return Ok(new
        {
            Balance = balance
        });
    }
}
