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
[Route("api/loyalty-programs")]
public class LoyaltyProgramController(
    ILoyaltyProgramService crudService,
    ILoyaltyProgramSearchService searchService)
    : Controller
{
    [HttpPost("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgramSearchResult>> Search([FromBody] LoyaltyProgramSearchCriteria criteria)
    {
        var result = await searchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Permissions.Create)]
    public Task<ActionResult<LoyaltyProgram>> Create([FromBody] LoyaltyProgram model)
    {
        model.Id = null;
        return Update(model);
    }

    [HttpPut]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<LoyaltyProgram>> Update([FromBody] LoyaltyProgram model)
    {
        await crudService.SaveChangesAsync([model]);
        return Ok(model);
    }

    [HttpGet("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<LoyaltyProgram>> Get([FromRoute] string id, [FromQuery] string responseGroup = null)
    {
        var model = await crudService.GetNoCloneAsync(id, responseGroup);

        if (model != null)
        {
            switch (model.ProgramType)
            {
                case ModuleConstants.LoyaltyPrograms.ProductProgramType:
                    model.DynamicExpression?.MergeFromPrototype(AbstractTypeFactory<LoyaltyProgramProductConditionTreePrototype>.TryCreateInstance());
                    break;
                default:
                    model.DynamicExpression?.MergeFromPrototype(AbstractTypeFactory<LoyaltyProgramConditionAndRewardTreePrototype>.TryCreateInstance());
                    break;
            }
        }

        return Ok(model);
    }

    [HttpGet]
    [Route("new/{programType}")]
    [Authorize(Permissions.Create)]
    public ActionResult<LoyaltyProgram> GetNewLoyaltyProgram([FromRoute] string programType)
    {
        var result = AbstractTypeFactory<LoyaltyProgram>.TryCreateInstance();

        switch (programType)
        {
            case ModuleConstants.LoyaltyPrograms.ProductProgramType:
                result.DynamicExpression.MergeFromPrototype(AbstractTypeFactory<LoyaltyProgramProductConditionTreePrototype>.TryCreateInstance());
                break;
            default:
                result.DynamicExpression.MergeFromPrototype(AbstractTypeFactory<LoyaltyProgramConditionAndRewardTreePrototype>.TryCreateInstance());
                break;
        }

        result.IsActive = true;

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
