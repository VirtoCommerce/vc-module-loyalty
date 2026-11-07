using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Handlers;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.Loyalty.Data.BackgroundJobs;

// Each event-driven job replaces Hangfire's [DisableConcurrentExecution(10)] on the handler method with the same 10-second
// wait on a distributed lock. Like the Hangfire attribute, a timeout fails the job and the engine retries it, so no accrual
// is skipped; unlike it, the lock spans the whole worker fleet, not one Hangfire server. The logic stays on the event
// handlers' public methods, which existing customizations may override, and jobs queued by an earlier version still
// target those methods.
internal static class LoyaltyJobLocks
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
}

/// <summary>
/// Payload of <see cref="ProcessLoyaltyOrdersJob"/>: the newly placed orders to accrue loyalty points for.
/// </summary>
public class ProcessLoyaltyOrdersJobPayload
{
    public LoyaltyOrdersContext Context { get; set; }
}

/// <summary>
/// Accrues loyalty points for newly placed orders, off the request thread.
/// </summary>
public class ProcessLoyaltyOrdersJob(LoyaltyProgramHandler loyaltyProgramHandler, IDistributedLock distributedLock)
    : IBackgroundJobHandler<ProcessLoyaltyOrdersJobPayload>
{
    private const string LockResource = "loyalty:job:process-orders";

    public virtual Task Execute(ProcessLoyaltyOrdersJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return distributedLock.ExecuteAsync(LockResource, _ => loyaltyProgramHandler.ProcessOrdersAsync(payload.Context), LoyaltyJobLocks.Timeout, cancellationToken);
    }
}

/// <summary>
/// Payload of <see cref="ProcessLoyaltyUsersJob"/>: the newly registered users to process.
/// </summary>
public class ProcessLoyaltyUsersJobPayload
{
    public LoyaltyApplicationUserContext Context { get; set; }
}

/// <summary>
/// Processes loyalty rules for newly registered users, off the request thread.
/// </summary>
public class ProcessLoyaltyUsersJob(LoyaltyProgramHandler loyaltyProgramHandler, IDistributedLock distributedLock)
    : IBackgroundJobHandler<ProcessLoyaltyUsersJobPayload>
{
    private const string LockResource = "loyalty:job:process-users";

    public virtual Task Execute(ProcessLoyaltyUsersJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return distributedLock.ExecuteAsync(LockResource, _ => loyaltyProgramHandler.ProcessApplicationUsersAsync(payload.Context), LoyaltyJobLocks.Timeout, cancellationToken);
    }
}

/// <summary>
/// Payload of <see cref="ProcessLoyaltyMissionsJob"/>: the newly placed orders to evaluate missions for.
/// </summary>
public class ProcessLoyaltyMissionsJobPayload
{
    public string[] OrderIds { get; set; } = [];
}

/// <summary>
/// Evaluates loyalty missions for newly placed orders, off the request thread.
/// </summary>
public class ProcessLoyaltyMissionsJob(LoyaltyMissionHandler loyaltyMissionHandler, IDistributedLock distributedLock)
    : IBackgroundJobHandler<ProcessLoyaltyMissionsJobPayload>
{
    private const string LockResource = "loyalty:job:process-missions";

    public virtual Task Execute(ProcessLoyaltyMissionsJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return distributedLock.ExecuteAsync(LockResource, _ => loyaltyMissionHandler.ProcessMissionsAsync(payload.OrderIds), LoyaltyJobLocks.Timeout, cancellationToken);
    }
}

/// <summary>
/// Payload of <see cref="ExpireLoyaltyMissionsJob"/>. Carries no data: each occurrence expires progress of every ended mission.
/// </summary>
public class ExpireLoyaltyMissionsJobPayload
{
}

/// <summary>
/// Expires incomplete mission progress for ended missions, on the daily recurring schedule.
/// </summary>
public class ExpireLoyaltyMissionsJob(ILoyaltyMissionLogicService missionLogicService)
    : IBackgroundJobHandler<ExpireLoyaltyMissionsJobPayload>
{
    public virtual Task Execute(ExpireLoyaltyMissionsJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return missionLogicService.ExpireMissionsAsync();
    }
}
