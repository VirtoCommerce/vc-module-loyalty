using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Loyalty.ExperienceApi.Queries;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
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
        private readonly IMemberResolver _memberResolver;

        public CanAccessLoyaltyAuthorizationHandler(IMemberResolver memberResolver)
        {
            _memberResolver = memberResolver;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanAccessLoyaltyAuthorizationRequirement requirement)
        {
            var result = context.User.IsInRole(PlatformConstants.Security.SystemRoles.Administrator);

            if (!result)
            {
                var userId = GetCurrentUserId(context);
                switch (context.Resource)
                {
                    case CustomerOrder order:
                        result = order.CustomerId == userId;
                        break;
                    case ILoyaltyQuery query:
                        if (!query.OrganizationId.IsNullOrEmpty())
                        {
                            if (await _memberResolver.ResolveMemberByIdAsync(userId) is IHasOrganizations member)
                            {
                                result = member.Organizations?.Contains(query.OrganizationId) == true;
                            }
                        }
                        else if (!query.UserId.IsNullOrEmpty())
                        {
                            result = query.UserId == userId;
                        }
                        break;
                    case GetMissionProgressQuery query:
                        result = query.UserId == userId;
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
        }

        private static string GetCurrentUserId(AuthorizationHandlerContext context)
        {
            return context.User.GetUserId();
        }
    }
}
