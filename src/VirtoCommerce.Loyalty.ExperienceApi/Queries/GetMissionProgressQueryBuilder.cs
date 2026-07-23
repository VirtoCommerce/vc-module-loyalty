using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.ExperienceApi.Authorization;
using VirtoCommerce.Loyalty.ExperienceApi.Schemas;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Security.Authorization;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetMissionProgressQueryBuilder : SearchQueryBuilder<GetMissionProgressQuery, LoyaltyUserMissionSearchResult, LoyaltyUserMission, LoyaltyUserMissionType>
{
    protected override string Name => "loyaltyMissionProgress";

    private readonly ICurrencyService _currencyService;

    public GetMissionProgressQueryBuilder(IMediator mediator, IAuthorizationService authorizationService, ICurrencyService currencyService)
        : base(mediator, authorizationService)
    {
        _currencyService = currencyService;
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

    protected override async Task AfterMediatorSend(IResolveFieldContext<object> context, GetMissionProgressQuery request, LoyaltyUserMissionSearchResult response)
    {
        var currencies = await _currencyService.GetAllCurrenciesAsync();
        context.SetCurrencies(currencies, request.CultureName);

        foreach (var loyaltyUserMission in response.Results)
        {
            context.SetExpandedObjectGraph(loyaltyUserMission);
        }

        await base.AfterMediatorSend(context, request, response);
    }
}
