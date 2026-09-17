using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Extensions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetLoyaltyBalanceQueryBuilderHandler : IQueryHandler<GetLoyaltyBalanceQuery, LoyaltyBalanceResult>
{
    private readonly ILoyaltyLogicService _loyaltyLogicService;
    private readonly IStoreService _storeService;

    public GetLoyaltyBalanceQueryBuilderHandler(ILoyaltyLogicService loyaltyLogicService, IStoreService storeService)
    {
        _loyaltyLogicService = loyaltyLogicService;
        _storeService = storeService;
    }

    public async Task<LoyaltyBalanceResult> Handle(GetLoyaltyBalanceQuery request, CancellationToken cancellationToken)
    {
        var balanceRequest = await GetLoyaltyBalanceRequestAsync(request);
        var result = await _loyaltyLogicService.GetLoyaltyBalanceAsync(balanceRequest);

        return result;
    }

    protected virtual async Task<LoyaltyBalanceRequest> GetLoyaltyBalanceRequestAsync(GetLoyaltyBalanceQuery request)
    {
        var balanceRequest = AbstractTypeFactory<LoyaltyBalanceRequest>.TryCreateInstance();

        balanceRequest.OrderId = request.OrderId;

        var organizationId = await ResolveOrganizationIdAsync(request.StoreId, request.OrganizationId);

        if (!organizationId.IsNullOrEmpty())
        {
            balanceRequest.OrganizationId = organizationId;
        }
        else
        {
            balanceRequest.UserId = request.UserId;
        }

        return balanceRequest;
    }

    // The organization scope applies only to a store that calculates loyalty per organization.
    protected virtual async Task<string> ResolveOrganizationIdAsync(string storeId, string organizationId)
    {
        if (organizationId.IsNullOrEmpty() || storeId.IsNullOrEmpty())
        {
            return null;
        }

        var store = await _storeService.GetNoCloneAsync(storeId);

        return store?.IsOrganizationBalanceCalculationMode() == true ? organizationId : null;
    }
}
