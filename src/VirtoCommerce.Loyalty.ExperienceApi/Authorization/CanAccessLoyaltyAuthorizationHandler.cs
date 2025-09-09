using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Loyalty.ExperienceApi.Queries;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.Loyalty.ExperienceApi.Authorization
{
    public sealed class CanAccessLoyaltyAuthorizationRequirement : PermissionAuthorizationRequirement
    {
        public CanAccessLoyaltyAuthorizationRequirement() : base("CanAccessLoyalty")
        {
        }
    }

    public class CanAccessLoyaltyAuthorizationHandler : PermissionAuthorizationHandlerBase<CanAccessLoyaltyAuthorizationRequirement>
    {
        public CanAccessLoyaltyAuthorizationHandler()
        {
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanAccessLoyaltyAuthorizationRequirement requirement)
        {
            var result = context.User.IsInRole(PlatformConstants.Security.SystemRoles.Administrator);

            if (!result)
            {
                switch (context.Resource)
                {
                    case CustomerOrder order:
                        result = order.CustomerId == GetCurrentUserId(context);
                        break;
                    case GetLoyaltyHistoryQuery query:
                        result = query.UserId == GetCurrentUserId(context);
                        break;
                }
            }

            if (result)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }

        private static string GetCurrentUserId(AuthorizationHandlerContext context)
        {
            return context.User.GetUserId();
        }
    }
}
