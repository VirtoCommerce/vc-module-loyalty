using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.ExperienceApi.Queries;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.QuoteModule.ExperienceApi.Queries;

public class GetLoyaltyHistoryQueryHandler : IQueryHandler<GetLoyaltyHistoryQuery, LoyaltyBalanceOperationLogSearchResult>
{
    private readonly ILoyaltyBalanceOperationLogSearchService _loyaltyLogSearchService;

    public GetLoyaltyHistoryQueryHandler(ILoyaltyBalanceOperationLogSearchService loyaltyLogSearchService)
    {
        _loyaltyLogSearchService = loyaltyLogSearchService;
    }

    public virtual async Task<LoyaltyBalanceOperationLogSearchResult> Handle(GetLoyaltyHistoryQuery request, CancellationToken cancellationToken)
    {
        var criteria = GetSearchCriteria(request);

        var searchResult = await _loyaltyLogSearchService.SearchAsync(criteria);

        return searchResult;
    }

    protected virtual LoyaltyBalanceOperationLogSearchCriteria GetSearchCriteria(GetLoyaltyHistoryQuery request)
    {
        var criteria = request.GetSearchCriteria<LoyaltyBalanceOperationLogSearchCriteria>();
        criteria.UserId = request.UserId;
        criteria.OperationType = request.OperationType;

        return criteria;
    }
}
