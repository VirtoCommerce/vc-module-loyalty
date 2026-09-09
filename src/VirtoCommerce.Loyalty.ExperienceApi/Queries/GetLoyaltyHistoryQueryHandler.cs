using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Extensions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetLoyaltyHistoryQueryHandler : IQueryHandler<GetLoyaltyHistoryQuery, LoyaltyBalanceOperationLogSearchResult>
{
    private readonly ILoyaltyBalanceOperationLogSearchService _loyaltyLogSearchService;
    private readonly IStoreService _storeService;

    public GetLoyaltyHistoryQueryHandler(ILoyaltyBalanceOperationLogSearchService loyaltyLogSearchService, IStoreService storeService)
    {
        _loyaltyLogSearchService = loyaltyLogSearchService;
        _storeService = storeService;
    }

    public virtual async Task<LoyaltyBalanceOperationLogSearchResult> Handle(GetLoyaltyHistoryQuery request, CancellationToken cancellationToken)
    {
        var criteria = await GetSearchCriteriaAsync(request);

        var searchResult = await _loyaltyLogSearchService.SearchAsync(criteria);

        return searchResult;
    }

    protected virtual async Task<LoyaltyBalanceOperationLogSearchCriteria> GetSearchCriteriaAsync(GetLoyaltyHistoryQuery request)
    {
        var criteria = request.GetSearchCriteria<LoyaltyBalanceOperationLogSearchCriteria>();

        criteria.OperationType = request.OperationType;

        var organizationId = await ResolveOrganizationIdAsync(request.StoreId, request.OrganizationId);

        if (!organizationId.IsNullOrEmpty())
        {
            criteria.OrganizationId = organizationId;
        }
        else
        {
            criteria.UserId = request.UserId;
        }

        return criteria;
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
