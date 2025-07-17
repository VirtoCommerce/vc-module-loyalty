using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Models;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Permissions = VirtoCommerce.Loyalty.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.Loyalty.Web.Controllers.Api;

[Authorize]
[Route("api/loyalty")]
public class LoyaltyController(
        ILoyaltyProgramService loyaltyService,
        ILoyaltyProgramSearchService searchService,
        ITransactionLogService transactionService,
        ITransactionLogSearchService transactionSearchService) : Controller
{
    [HttpPost("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgramSearchResult>> Search([FromBody] LoyaltyProgramSearchCriteria criteria)
    {
        var result = await searchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpPost]
    [Route("")]
    [Authorize(Permissions.Create)]
    public async Task<ActionResult<LoyaltyProgram>> Create([FromBody] LoyaltyProgram loyaltyProgram)
    {
        loyaltyProgram.Id = null;
        await loyaltyService.SaveChangesAsync([loyaltyProgram]);
        return Ok(loyaltyProgram);
    }

    [HttpGet]
    [Route("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgram>> Get([FromRoute] string id)
    {
        var result = await loyaltyService.GetNoCloneAsync(id);
        return Ok(result);
    }

    [HttpPut]
    [Route("")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<LoyaltyProgram>> Update([FromBody] LoyaltyProgram loyaltyProgram)
    {
        await loyaltyService.SaveChangesAsync([loyaltyProgram]);
        return Ok(loyaltyProgram);
    }

    [HttpDelete]
    [Route("")]
    [Authorize(Permissions.Delete)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        await loyaltyService.DeleteAsync(ids);
        return NoContent();
    }

    [HttpGet]
    [Route("new")]
    [Authorize(Permissions.Create)]
    public ActionResult<LoyaltyProgram> GetNewLoyaltyProgram()
    {
        var retVal = AbstractTypeFactory<LoyaltyProgram>.TryCreateInstance();
        return Ok(retVal);
    }

    [HttpGet]
    [Route("points/{customerId}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<TransactionLogPointModel>> GetPointsByCustomerId([FromRoute] string customerId)
    {
        var totalPoints = await transactionService.GetPointsByCustomerIdAsync(customerId, transactionSearchService);
        return Ok(new TransactionLogPointModel(totalPoints));
    }

    [HttpPost("transactions/search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<TransactionLogSearchResult>> TransactionSearch([FromBody] TransactionLogSearchCriteria criteria)
    {
        var result = await transactionSearchService.SearchAsync(criteria);
        return Ok(result);
    }
}
