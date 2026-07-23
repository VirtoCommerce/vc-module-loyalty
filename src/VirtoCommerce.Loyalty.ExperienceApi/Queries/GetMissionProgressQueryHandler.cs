using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetMissionProgressQueryHandler : IQueryHandler<GetMissionProgressQuery, LoyaltyUserMissionSearchResult>
{
    private readonly ILoyaltyMissionLogicService _missionLogicService;

    public GetMissionProgressQueryHandler(ILoyaltyMissionLogicService missionLogicService)
    {
        _missionLogicService = missionLogicService;
    }

    public virtual async Task<LoyaltyUserMissionSearchResult> Handle(GetMissionProgressQuery request, CancellationToken cancellationToken)
    {
        var pagingCriteria = request.GetSearchCriteria<LoyaltyMissionProgressSearchCriteria>();

        var criteria = GetLoyaltyUserMissionCriteria(request);

        var userMissions = await _missionLogicService.GetUserMissionsAsync(criteria);

        var result = AbstractTypeFactory<LoyaltyUserMissionSearchResult>.TryCreateInstance();
        result.TotalCount = userMissions.Count;
        result.Results = userMissions.Skip(pagingCriteria.Skip).Take(pagingCriteria.Take).ToList();

        return result;
    }

    private static LoyaltyUserMissionSearchCriteria GetLoyaltyUserMissionCriteria(GetMissionProgressQuery request)
    {
        var criteria = AbstractTypeFactory<LoyaltyUserMissionSearchCriteria>.TryCreateInstance();

        criteria.UserId = request.UserId;
        criteria.StoreId = request.StoreId;
        criteria.Statuses = request.Statuses;
        criteria.CompletedStartDate = request.CompletedStartDate;
        criteria.CompletedEndDate = request.CompletedEndDate;
        criteria.IsStarted = request.IsStarted;

        return criteria;
    }
}
