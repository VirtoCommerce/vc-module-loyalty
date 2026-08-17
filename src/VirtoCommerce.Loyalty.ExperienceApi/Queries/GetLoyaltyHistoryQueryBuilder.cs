using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.ExperienceApi.Authorization;
using VirtoCommerce.Loyalty.ExperienceApi.Queries;
using VirtoCommerce.Loyalty.ExperienceApi.Schemas;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Security.Authorization;

namespace VirtoCommerce.QuoteModule.ExperienceApi.Queries;

public class GetLoyaltyHistoryQueryBuilder : SearchQueryBuilder<GetLoyaltyHistoryQuery, LoyaltyBalanceOperationLogSearchResult, LoyaltyBalanceOperationLog, LoyaltyOperationLogType>
{
    protected override string Name => "loyaltyPointsHistory";

    public GetLoyaltyHistoryQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : base(mediator, authorizationService)
    {
    }

    protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, GetLoyaltyHistoryQuery request)
    {
        if (!context.IsAuthenticated())
        {
            throw AuthorizationError.AnonymousAccessDenied();
        }

        await Authorize(context, request, new CanAccessLoyaltyAuthorizationRequirement());

        await base.BeforeMediatorSend(context, request);
    }
}
