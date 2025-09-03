using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.ExperienceApi.Schemas;
using VirtoCommerce.Xapi.Core.BaseQueries;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetLoyaltyBalanceQueryBuilder : QueryBuilder<GetLoyaltyBalanceQuery, LoyaltyBalanceResult, LoyaltyBalanceResultType>
{
    public GetLoyaltyBalanceQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : base(mediator, authorizationService)
    {
    }

    protected override string Name => "loyaltyBalance";
}
