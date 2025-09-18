using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.ExperienceApi.Queries;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.QuoteModule.ExperienceApi.Queries;

public class GetLoyaltyHistoryQueryHandler : IQueryHandler<GetLoyaltyHistoryQuery, LoyaltyProgramOperationLogSearchResult>
{
    private readonly ILoyaltyProgramOperationLogSearchService _loyaltyLogSearchService;

    public GetLoyaltyHistoryQueryHandler(ILoyaltyProgramOperationLogSearchService loyaltyLogSearchService)
    {
        _loyaltyLogSearchService = loyaltyLogSearchService;
    }

    public virtual async Task<LoyaltyProgramOperationLogSearchResult> Handle(GetLoyaltyHistoryQuery request, CancellationToken cancellationToken)
    {
        var criteria = GetSearchCriteria(request);

        var searchResult = await _loyaltyLogSearchService.SearchAsync(criteria);

        return searchResult;
    }

    protected virtual LoyaltyProgramOperationLogSearchCriteria GetSearchCriteria(GetLoyaltyHistoryQuery request)
    {
        var criteria = request.GetSearchCriteria<LoyaltyProgramOperationLogSearchCriteria>();
        criteria.UserId = request.UserId;
        criteria.OperationType = request.OperationType;

        return criteria;
    }
}
