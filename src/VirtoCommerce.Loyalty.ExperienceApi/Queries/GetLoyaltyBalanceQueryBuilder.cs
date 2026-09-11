using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.ExperienceApi.Authorization;
using VirtoCommerce.Loyalty.ExperienceApi.Schemas;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Security.Authorization;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetLoyaltyBalanceQueryBuilder : QueryBuilder<GetLoyaltyBalanceQuery, LoyaltyBalanceResult, LoyaltyBalanceResultType>
{
    private readonly ICustomerOrderService _customerOrderService;

    public GetLoyaltyBalanceQueryBuilder(
        IMediator mediator,
        IAuthorizationService authorizationService,
        ICustomerOrderService customerOrderService)
        : base(mediator, authorizationService)
    {
        _customerOrderService = customerOrderService;
    }

    protected override string Name => "loyaltyBalance";

    protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, GetLoyaltyBalanceQuery request)
    {
        if (!context.IsAuthenticated())
        {
            throw AuthorizationError.AnonymousAccessDenied();
        }

        await Authorize(context, request, new CanAccessLoyaltyAuthorizationRequirement());

        if (!request.OrderId.IsNullOrEmpty())
        {
            var order = await _customerOrderService.GetNoCloneAsync(request.OrderId, CustomerOrderResponseGroup.WithPrices.ToString());
            await Authorize(context, order, new CanAccessLoyaltyAuthorizationRequirement());
        }

        await base.BeforeMediatorSend(context, request);
    }
}
