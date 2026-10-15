using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Data.Handlers;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.Loyalty.Data.BackgroundJobs;

/// <summary>
/// Processes loyalty missions for newly placed orders, off the request thread. The logic stays on
/// <see cref="LoyaltyMissionHandler.ProcessMissionsAsync"/>, which jobs queued by an earlier version still reference.
/// </summary>
public class ProcessLoyaltyMissionsJobHandler(LoyaltyMissionHandler loyaltyMissionHandler, IDistributedLock distributedLock)
    : IBackgroundJobHandler<ProcessLoyaltyMissionsJobPayload>
{
    // Replaces Hangfire's [DisableConcurrentExecution(10)] with a distributed lock and the same 10-second wait. As with
    // Hangfire, a job that cannot get the lock in time fails and the engine retries it: skipping instead would lose the
    // mission progress of the orders.
    private const string LockResource = "loyalty:job:process-missions";
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(10);

    public virtual Task Execute(ProcessLoyaltyMissionsJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return distributedLock.ExecuteAsync(LockResource, _ => loyaltyMissionHandler.ProcessMissionsAsync(payload.OrderIds), _lockTimeout, cancellationToken);
    }
}
