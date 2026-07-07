using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.ExperienceApi.Authorization;
using VirtoCommerce.Loyalty.ExperienceApi.Schemas;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Security.Authorization;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetMissionProgressQueryBuilder : SearchQueryBuilder<GetMissionProgressQuery, LoyaltyMissionProgressSearchResult, LoyaltyMissionProgress, LoyaltyMissionProgressType>
{
    protected override string Name => "loyaltyMissionProgress";

    public GetMissionProgressQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : base(mediator, authorizationService)
    {
    }

    protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, GetMissionProgressQuery request)
    {
        if (!context.IsAuthenticated())
        {
            throw AuthorizationError.AnonymousAccessDenied();
        }

        await Authorize(context, request, new CanAccessLoyaltyAuthorizationRequirement());

        context.CopyArgumentsToUserContext();
        await base.BeforeMediatorSend(context, request);
    }
}
