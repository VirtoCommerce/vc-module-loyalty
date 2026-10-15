using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.Loyalty.Data.BackgroundJobs;

/// <summary>
/// Moves in-progress records of ended missions to the Expired status, on the recurring schedule.
/// </summary>
public class ExpireLoyaltyMissionsJobHandler(ILoyaltyMissionLogicService missionLogicService)
    : IBackgroundJobHandler<ExpireLoyaltyMissionsJobPayload>
{
    public virtual Task Execute(ExpireLoyaltyMissionsJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return missionLogicService.ExpireMissionsAsync();
    }
}
