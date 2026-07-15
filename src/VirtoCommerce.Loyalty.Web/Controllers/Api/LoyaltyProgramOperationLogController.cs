using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using Permissions = VirtoCommerce.Loyalty.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.Loyalty.Web.Controllers.Api;

[Authorize]
[Route("api/loyalty-program-operation-log")]
public class LoyaltyProgramOperationLogController(
    ILoyaltyProgramOperationLogService operationLogservice,
    ILoyaltyProgramOperationLogSearchService searchService,
    ILoyaltyLogicService loyaltyLogicService,
    UserManager<ApplicationUser> userManager)
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

    [HttpPost]
    public async Task<ActionResult<LoyaltyProgramOperationLog>> AddOperationLog([FromBody] UserLoyaltyProgramOperationLog userOperationLog)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser?.IsAdministrator != true)
        {
            return Forbid();
        }

        var models = new List<LoyaltyProgramOperationLog>();
        foreach (var userName in userOperationLog.UserNames)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                continue;
            }

            var operationLog = new LoyaltyProgramOperationLog
            {
                UserId = user.Id,
                Amount = userOperationLog.Amount,
                Balance = userOperationLog.Balance,
                ObjectId = userOperationLog.ObjectId ?? user.Id,
                ObjectType = userOperationLog.ObjectType,
                OperationType = userOperationLog.OperationType,
            };
            models.Add(operationLog);
        }

        await operationLogservice.SaveChangesAsync(models);

        return Ok();
    }

    private Task<ApplicationUser> GetCurrentUserAsync()
    {
        if (string.IsNullOrEmpty(User.Identity?.Name) || !User.Identity.IsAuthenticated)
        {
            return Task.FromResult<ApplicationUser>(null);
        }

        return userManager.FindByNameAsync(User.Identity.Name);
    }

    public class UserLoyaltyProgramOperationLog
    {
        public IList<string> UserNames { get; set; } = [];

        public decimal Amount { get; set; }

        public decimal Balance { get; set; }

        public string ObjectId { get; set; }

        public string ObjectType { get; set; }

        public string OperationType { get; set; }
    }
}
