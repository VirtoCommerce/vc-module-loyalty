using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Data.Handlers;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.Loyalty.Data.BackgroundJobs;

/// <summary>
/// Awards registration loyalty points to newly created users, off the request thread. The logic stays on
/// <see cref="LoyaltyProgramHandler.ProcessApplicationUsersAsync"/>, which jobs queued by an earlier version still reference.
/// </summary>
public class ProcessLoyaltyApplicationUsersJobHandler(LoyaltyProgramHandler loyaltyProgramHandler, IDistributedLock distributedLock)
    : IBackgroundJobHandler<ProcessLoyaltyApplicationUsersJobPayload>
{
    // Replaces Hangfire's [DisableConcurrentExecution(10)] with a distributed lock and the same 10-second wait. As with
    // Hangfire, a job that cannot get the lock in time fails and the engine retries it: skipping instead would lose the
    // registration points of the users.
    private const string LockResource = "loyalty:job:process-application-users";
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(10);

    public virtual Task Execute(ProcessLoyaltyApplicationUsersJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var usersContext = new LoyaltyApplicationUserContext
        {
            ApplicationUserIds = payload.ApplicationUserIds,
        };

        return distributedLock.ExecuteAsync(LockResource, _ => loyaltyProgramHandler.ProcessApplicationUsersAsync(usersContext), _lockTimeout, cancellationToken);
    }
}
