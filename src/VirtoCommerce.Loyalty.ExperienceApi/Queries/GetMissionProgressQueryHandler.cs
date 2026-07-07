using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetMissionProgressQueryHandler : IQueryHandler<GetMissionProgressQuery, LoyaltyMissionProgressSearchResult>
{
    private readonly ILoyaltyMissionProgressSearchService _missionProgressSearchService;

    public GetMissionProgressQueryHandler(ILoyaltyMissionProgressSearchService missionProgressSearchService)
    {
        _missionProgressSearchService = missionProgressSearchService;
    }

    public virtual async Task<LoyaltyMissionProgressSearchResult> Handle(GetMissionProgressQuery request, CancellationToken cancellationToken)
    {
        var criteria = GetSearchCriteria(request);

        var searchResult = await _missionProgressSearchService.SearchAsync(criteria);

        return searchResult;
    }

    protected virtual LoyaltyMissionProgressSearchCriteria GetSearchCriteria(GetMissionProgressQuery request)
    {
        var criteria = request.GetSearchCriteria<LoyaltyMissionProgressSearchCriteria>();
        criteria.UserId = request.UserId;
        criteria.Status = request.Status;
        criteria.StoreId = request.StoreId;

        return criteria;
    }
}
