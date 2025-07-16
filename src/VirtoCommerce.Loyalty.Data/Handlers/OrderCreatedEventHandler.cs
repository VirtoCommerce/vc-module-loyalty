using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Models;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Services;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Data.Handlers;

public class OrderCreatedEventHandler(
        ILoyaltyProgramSearchService loyaltySearchService,
        ITransactionLogSearchService transactionSearchService,
        ITransactionLogService transactionService)
    : IEventHandler<OrderChangedEvent>
{
    private const decimal DefaultPointsPerOrder = 50m; // Default points to be awarded per order

    public Task Handle(OrderChangedEvent message)
    {
        var orders = message.ChangedEntries.Where(x => x.EntryState == EntryState.Added)
            .Select(x => x.NewEntry).ToList();
        if (orders.Any())
        {
            BackgroundJob.Enqueue(() => HandleLoyaltyProgramUsages(orders));
        }
        return Task.CompletedTask;
    }

    [DisableConcurrentExecution(10)]
    public async Task HandleLoyaltyProgramUsages(IEnumerable<CustomerOrder> orders)
    {
        var storeIds = orders.Select(o => o.StoreId).Distinct().ToArray();

        var loyaltyProgramCriteria = AbstractTypeFactory<LoyaltyProgramSearchCriteria>.TryCreateInstance();
        loyaltyProgramCriteria.StoreIds = storeIds;
        loyaltyProgramCriteria.IsActive = true;

        var loyaltyPrograms = (await loyaltySearchService.SearchAsync(loyaltyProgramCriteria)).Results;
        if (loyaltyPrograms?.Any() != true)
        {
            return;
        }

        var usageContexts = orders
            .Select(order =>
            {
                var matchedProgram = loyaltyPrograms
                    .Where(program => program.StoreIds.Contains(order.StoreId))
                    .OrderBy(p => p.Priority)
                    .FirstOrDefault();

                return matchedProgram != null
                    ? new { Order = order, LoyaltyProgram = matchedProgram }
                    : null;
            })
            .Where(x => x != null)
            .ToList();

        var customerBalances = new Dictionary<string, decimal>();
        var distinctCustomerIds = usageContexts.Select(x => x.Order.CustomerId).Distinct();
        foreach (var customerId in distinctCustomerIds)
        {
            await InitializeCustomerBalanceAsync(customerBalances, customerId);
        }

        var transactionLogs = new List<TransactionLog>();
        foreach (var usageContext in usageContexts)
        {
            customerBalances[usageContext.Order.CustomerId] += DefaultPointsPerOrder;
            transactionLogs.Add(new TransactionLog
            {
                LoyaltyProgramId = usageContext.LoyaltyProgram.Id,
                CustomerId = usageContext.Order.CustomerId,
                ObjectId = usageContext.Order.Id,
                ObjectType = nameof(CustomerOrder),
                Points = DefaultPointsPerOrder, // Assuming a fixed point value for simplicity
                OperationType = LoyaltyOperationType.Debit,
                Balance = customerBalances[usageContext.Order.CustomerId],
                Comment = $"Order #{usageContext.Order.Number} processed for loyalty program {usageContext.LoyaltyProgram.Name}",
            });
        }
        if (transactionLogs.Any())
        {
            await transactionService.SaveChangesAsync(transactionLogs);
        }
    }

    private async Task InitializeCustomerBalanceAsync(Dictionary<string, decimal> customerBalances, string customerId)
    {
        var transactionCriteria = AbstractTypeFactory<TransactionLogSearchCriteria>.TryCreateInstance();
        transactionCriteria.CustomerId = customerId;
        var transactions = (await transactionSearchService.SearchAsync(transactionCriteria)).Results;
        var lastTransaction = transactions.OrderByDescending(t => t.CreatedDate).FirstOrDefault();
        customerBalances[customerId] = lastTransaction?.Balance ?? 0m;
    }
}
