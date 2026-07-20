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
        var criteria = request.GetSearchCriteria<LoyaltyMissionProgressSearchCriteria>();

        // Qualifying published missions paired with the user's progress (transient 0% when not started).
        var userMissions = await _missionLogicService.GetUserMissionsAsync(request.UserId, request.StoreId, request.Statuses, request.CompletedStartDate, request.CompletedEndDate, request.IsStarted);

        var result = AbstractTypeFactory<LoyaltyUserMissionSearchResult>.TryCreateInstance();
        result.TotalCount = userMissions.Count;
        result.Results = userMissions.Skip(criteria.Skip).Take(criteria.Take).ToList();

        return result;
    }
}
