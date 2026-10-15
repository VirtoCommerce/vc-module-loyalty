using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Data.Handlers;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.Loyalty.Data.BackgroundJobs;

/// <summary>
/// Awards and redeems loyalty points for newly placed orders, off the request thread. The logic stays on
/// <see cref="LoyaltyProgramHandler.ProcessOrdersAsync"/>, which jobs queued by an earlier version still reference.
/// </summary>
public class ProcessLoyaltyOrdersJobHandler(LoyaltyProgramHandler loyaltyProgramHandler, IDistributedLock distributedLock)
    : IBackgroundJobHandler<ProcessLoyaltyOrdersJobPayload>
{
    // Replaces Hangfire's [DisableConcurrentExecution(10)] with a distributed lock and the same 10-second wait. As with
    // Hangfire, a job that cannot get the lock in time fails and the engine retries it: skipping instead would lose the
    // loyalty points of the orders.
    private const string LockResource = "loyalty:job:process-orders";
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(10);

    public virtual Task Execute(ProcessLoyaltyOrdersJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var ordersContext = new LoyaltyOrdersContext
        {
            OrderIds = payload.OrderIds,
        };

        return distributedLock.ExecuteAsync(LockResource, _ => loyaltyProgramHandler.ProcessOrdersAsync(ordersContext), _lockTimeout, cancellationToken);
    }
}
