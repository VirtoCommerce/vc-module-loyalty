using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Extensions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Models.Missions;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.Loyalty.Data.Services;

public class LoyaltyMissionLogicService : ILoyaltyMissionLogicService
{
    private readonly ILoyaltyMissionSearchService _missionSearchService;
    private readonly ILoyaltyMissionGoalItemSearchService _goalItemSearchService;
    private readonly ILoyaltyMissionProgressService _progressService;
    private readonly ILoyaltyMissionProgressSearchService _progressSearchService;
    private readonly ILoyaltyMissionTransactionSearchService _transactionSearchService;
    private readonly ILoyaltyLogicService _loyaltyLogicService;
    private readonly IDistributedLockService _distributedLockService;
    private readonly IStoreService _storeService;

    public LoyaltyMissionLogicService(
        ILoyaltyMissionSearchService missionSearchService,
        ILoyaltyMissionGoalItemSearchService goalItemSearchService,
        ILoyaltyMissionProgressService progressService,
        ILoyaltyMissionProgressSearchService progressSearchService,
        ILoyaltyMissionTransactionSearchService transactionSearchService,
        ILoyaltyLogicService loyaltyLogicService,
        IDistributedLockService distributedLockService,
        IStoreService storeService)
    {
        _missionSearchService = missionSearchService;
        _goalItemSearchService = goalItemSearchService;
        _progressService = progressService;
        _progressSearchService = progressSearchService;
        _transactionSearchService = transactionSearchService;
        _loyaltyLogicService = loyaltyLogicService;
        _distributedLockService = distributedLockService;
        _storeService = storeService;
    }

    public async Task ProcessOrderAsync(CustomerOrder order, Store store)
    {
        if (order == null || store == null)
        {
            return;
        }

        if (!store.Settings.GetValue<bool>(ModuleConstants.Settings.General.MissionsEnable))
        {
            return;
        }

        // Missions are scoped exactly like balances: to the order organization when the store
        // calculates per organization, to the user otherwise (including an order without an organization).
        var organizationId = store.IsOrganizationBalanceCalculationMode() ? order.OrganizationId : null;

        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.ContextObjectType = nameof(CustomerOrder);
        context.OrderId = order.Id;
        await _loyaltyLogicService.PopulateLoyaltyProgramEvaluationContextAsync(context);
        context.OrganizationId = organizationId;

        var userId = context.UserId.IsNullOrEmpty() ? order.CustomerId : context.UserId;
        if (userId.IsNullOrEmpty())
        {
            return;
        }

        var criteria = AbstractTypeFactory<LoyaltyMissionSearchCriteria>.TryCreateInstance();
        criteria.StoreIds = [store.Id];
        criteria.OnlyActive = true;
        criteria.Take = 50;

        await foreach (var batch in _missionSearchService.SearchBatchesNoCloneAsync(criteria))
        {
            foreach (var mission in batch.Results)
            {
                if (!(mission.DynamicExpression?.IsSatisfiedBy(context) ?? false))
                {
                    continue;
                }

                var goal = ExtractGoal(mission.DynamicExpression);
                if (goal == null)
                {
                    continue;
                }

                await ApplyMissionAsync(mission, goal, order, userId, organizationId);
            }
        }
    }

    public async Task ExpireMissionsAsync()
    {
        var now = DateTime.UtcNow;

        var endedIds = new List<string>();

        var criteria = AbstractTypeFactory<LoyaltyMissionProgressSearchCriteria>.TryCreateInstance();
        criteria.Status = ModuleConstants.MissionProgressStatuses.InProgress;
        criteria.Take = 200;

        await foreach (var batch in _progressSearchService.SearchBatchesNoCloneAsync(criteria))
        {
            endedIds.AddRange(batch.Results
                .Where(x => x.PeriodEnd != null && x.PeriodEnd < now)
                .Select(x => x.Id));
        }

        foreach (var chunk in endedIds.Distinct().Chunk(50))
        {
            var progresses = (await _progressService.GetAsync(chunk)).ToList();
            foreach (var progress in progresses)
            {
                progress.Status = ModuleConstants.MissionProgressStatuses.Expired;
            }

            if (progresses.Count > 0)
            {
                await _progressService.SaveChangesAsync(progresses);
            }
        }
    }

    public async Task<IList<LoyaltyUserMission>> GetUserMissionsAsync(LoyaltyUserMissionSearchCriteria criteria)
    {
        var userId = criteria.UserId;
        var storeId = criteria.StoreId;
        var statuses = criteria.Statuses;
        var completedStartDate = criteria.CompletedStartDate;
        var completedEndDate = criteria.CompletedEndDate;
        var isStarted = criteria.IsStarted;
        var currencyCode = criteria.CurrencyCode;

        if (storeId.IsNullOrEmpty() || userId.IsNullOrEmpty())
        {
            return [];
        }

        var store = await _storeService.GetNoCloneAsync(storeId);
        if (store == null || !store.Settings.GetValue<bool>(ModuleConstants.Settings.General.MissionsEnable))
        {
            return [];
        }

        // Read the progress the contributions were actually written to: the organization's one in
        // organization mode (shared by all its members), the user's own otherwise. The store mode -
        // not the caller - decides, so a passed organizationId cannot redirect a user-scoped lookup.
        var organizationId = store.IsOrganizationBalanceCalculationMode() ? criteria.OrganizationId : null;
        var ownerId = ResolveOwnerId(userId, organizationId);

        var context = await GetLoyaltyContextAsync(userId, organizationId, storeId);

        // Search for all published missions that qualify for this user
        var qualifyingMissions = await GetQualifyingMissionsAsync(storeId, context);

        if (qualifyingMissions.Count == 0)
        {
            return [];
        }

        // Search for progress records for the qualifying missions
        var progressByMissionId = await GetProgressByMissionIdAsync(ownerId, qualifyingMissions);

        // Resolve the loyalty points currency (mission currency is resolved per mission from the OrderValue goal).
        var pointsCurrencyCode = store.GetLoyaltyCurrencyCode();

        // Pair every qualifying mission with its progress (real or transient 0%)
        var now = DateTime.UtcNow;
        var result = new List<LoyaltyUserMission>();

        foreach (var mission in qualifyingMissions)
        {
            var goal = ExtractGoal(mission.DynamicExpression);
            if (goal == null)
            {
                continue;
            }

            var progress = progressByMissionId.GetValueOrDefault(mission.Id);
            if (progress == null)
            {
                // Offer a transient 0% progress only for missions that can still be started (within the window).
                var inWindow = (mission.StartDate == null || mission.StartDate <= now)
                    && (mission.EndDate == null || mission.EndDate >= now);
                if (!inWindow)
                {
                    continue;
                }

                var goalItems = goal is PerSkuGoal ? await GetGoalItemsAsync(mission.Id) : [];
                progress = CreateTransientProgress(mission, goal, userId, organizationId, goalItems);
            }

            // Mission currency comes from the OrderValue goal (no fallback): null when not set or not an OrderValue goal.
            var missionCurrencyCode = goal is OrderValueGoal orderValueGoal ? orderValueGoal.CurrencyCode : null;

            result.Add(new LoyaltyUserMission
            {
                Mission = mission,
                Progress = progress,
                Store = store,
                MissionType = ResolveMissionType(goal),
                RewardPoints = GetRewardAmount(mission.DynamicExpression),
                MissionCurrencyCode = missionCurrencyCode,
                PointsCurrencyCode = pointsCurrencyCode,
            });
        }

        // Apply the requested progress-status filter.
        if (!statuses.IsNullOrEmpty())
        {
            result = result.Where(x => statuses.Contains(x.Progress.Status)).ToList();
        }

        // filter out OrderValue missions by requested currencyCode
        if (!currencyCode.IsNullOrEmpty())
        {
            result = result.Where(x => x.MissionCurrencyCode.IsNullOrEmpty() || x.MissionCurrencyCode.EqualsIgnoreCase(currencyCode)).ToList();
        }

        // Apply the CompletedDate range filter (keeps only missions completed within the range).
        if (completedStartDate != null)
        {
            result = result.Where(x => x.Progress.CompletedDate != null && x.Progress.CompletedDate >= completedStartDate).ToList();
        }

        if (completedEndDate != null)
        {
            result = result.Where(x => x.Progress.CompletedDate != null && x.Progress.CompletedDate <= completedEndDate).ToList();
        }

        // Apply the started/not-started filter (started = a real progress record exists).
        if (isStarted != null)
        {
            result = result.Where(x => !string.IsNullOrEmpty(x.Progress.Id) == isStarted.Value).ToList();
        }

        return result;
    }

    private async Task<LoyaltyProgramEvaluationContext> GetLoyaltyContextAsync(string userId, string organizationId, string storeId)
    {
        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.ContextObjectType = nameof(ApplicationUser);
        context.UserId = userId;
        context.StoreId = storeId;
        await _loyaltyLogicService.PopulateLoyaltyProgramEvaluationContextAsync(context);
        context.OrganizationId = organizationId;

        return context;
    }

    private async Task<List<LoyaltyMission>> GetQualifyingMissionsAsync(string storeId, LoyaltyProgramEvaluationContext context)
    {
        var missionCriteria = AbstractTypeFactory<LoyaltyMissionSearchCriteria>.TryCreateInstance();
        missionCriteria.StoreIds = [storeId];
        missionCriteria.Status = ModuleConstants.MissionStatuses.Published;
        missionCriteria.Take = 50;

        var qualifyingMissions = new List<LoyaltyMission>();
        await foreach (var batch in _missionSearchService.SearchBatchesNoCloneAsync(missionCriteria))
        {
            qualifyingMissions.AddRange(batch.Results
                .Where(x => x.DynamicExpression?.IsSatisfiedBy(context) ?? false));
        }

        return qualifyingMissions;
    }

    private async Task<Dictionary<string, LoyaltyMissionProgress>> GetProgressByMissionIdAsync(string ownerId, IList<LoyaltyMission> qualifyingMissions)
    {
        var progressByMissionId = new Dictionary<string, LoyaltyMissionProgress>(StringComparer.OrdinalIgnoreCase);
        var progressCriteria = AbstractTypeFactory<LoyaltyMissionProgressSearchCriteria>.TryCreateInstance();
        progressCriteria.OwnerId = ownerId;
        progressCriteria.MissionIds = qualifyingMissions.Select(x => x.Id).ToArray();
        progressCriteria.Take = 100;

        await foreach (var batch in _progressSearchService.SearchBatchesNoCloneAsync(progressCriteria))
        {
            foreach (var progress in batch.Results)
            {
                progressByMissionId.TryAdd(progress.MissionId, progress);
            }
        }

        return progressByMissionId;
    }

    private Task<bool> ApplyMissionAsync(LoyaltyMission mission, IMissionGoal goal, CustomerOrder order, string userId, string organizationId)
    {
        // Serialize per progress owner, not per user: in organization mode two members of the same
        // organization contribute to one shared progress, so a per-user lock would let them race it
        // (lost CurrentValue updates, and the mission completing - and rewarding - more than once).
        var ownerId = ResolveOwnerId(userId, organizationId);

        return _distributedLockService.ExecuteAsync($"loyalty-mission:{mission.Id}:{ownerId}",
            () => ApplyMissionInternalAsync(mission, goal, order, userId, organizationId),
            lockTimeout: TimeSpan.FromSeconds(30),
            tryLockTimeout: TimeSpan.FromSeconds(30),
            retryInterval: TimeSpan.FromMilliseconds(200));
    }

    private async Task<bool> ApplyMissionInternalAsync(LoyaltyMission mission, IMissionGoal goal, CustomerOrder order, string userId, string organizationId)
    {
        // Skip the order entirely (no transaction, no progress) when its currency does not match the OrderValue goal currency.
        if (goal is OrderValueGoal orderValueGoal
            && !orderValueGoal.CurrencyCode.IsNullOrEmpty()
            && !order.Currency.EqualsIgnoreCase(orderValueGoal.CurrencyCode))
        {
            return false;
        }

        var goalItems = goal is PerSkuGoal ? await GetGoalItemsAsync(mission.Id) : [];

        var progress = await GetOrCreateProgressAsync(mission, goal, userId, organizationId, goalItems);

        if (progress.Status.EqualsIgnoreCase(ModuleConstants.MissionProgressStatuses.Completed))
        {
            return false;
        }

        if (!await TransactionExistsAsync(mission.Id, order.Id))
        {
            var contribution = ApplyContribution(progress, goal, order);
            UpdateMissionProgressMetrics(progress, goal, contribution);

            var transaction = AbstractTypeFactory<LoyaltyMissionTransaction>.TryCreateInstance();
            transaction.Id = Guid.NewGuid().ToString("N");
            transaction.MissionId = mission.Id;
            transaction.MissionProgressId = progress.Id;
            transaction.UserId = userId;
            transaction.ObjectId = order.Id;
            transaction.ObjectType = nameof(CustomerOrder);
            transaction.ContributionValue = contribution;
            transaction.OrganizationId = organizationId;

            // transaction/progress atomic save
            progress.NewTransactions.Add(transaction);
            await _progressService.SaveChangesAsync([progress]);
            progress.NewTransactions.Clear();
        }

        if (IsCompleted(progress, goal))
        {
            await GrantRewardAsync(mission, progress, userId, organizationId);

            progress.Status = ModuleConstants.MissionProgressStatuses.Completed;
            progress.CompletedDate = DateTime.UtcNow;
            await _progressService.SaveChangesAsync([progress]);
        }

        return true;
    }

    private static bool IsCompleted(LoyaltyMissionProgress progress, IMissionGoal goal)
    {
        if (goal is PerSkuGoal perSkuGoal)
        {
            return perSkuGoal.All
                ? progress.Items.Count > 0 && progress.Items.All(x => x.CurrentQuantity >= x.TargetQuantity)
                : progress.Items.Any(x => x.CurrentQuantity >= x.TargetQuantity);
        }

        return progress.TargetValue > 0 && progress.CurrentValue >= progress.TargetValue;
    }

    private async Task<LoyaltyMissionProgress> GetOrCreateProgressAsync(
        LoyaltyMission mission,
        IMissionGoal goal,
        string userId,
        string organizationId,
        IList<LoyaltyMissionGoalItem> goalItems)
    {
        var (periodStart, periodEnd) = ResolvePeriod(mission);
        var ownerId = ResolveOwnerId(userId, organizationId);

        // Searched by owner so every member of an organization lands on the same progress record
        // (a UserId filter would give each member a private one and let them all complete the mission).
        var criteria = AbstractTypeFactory<LoyaltyMissionProgressSearchCriteria>.TryCreateInstance();
        criteria.MissionId = mission.Id;
        criteria.OwnerId = ownerId;
        criteria.Take = 100;

        var existing = (await _progressSearchService.SearchAsync(criteria)).Results;
        var progress = existing.FirstOrDefault(x => Nullable.Equals(x.PeriodStart, periodStart));

        if (progress != null)
        {
            return progress;
        }

        progress = AbstractTypeFactory<LoyaltyMissionProgress>.TryCreateInstance();
        progress.Id = Guid.NewGuid().ToString("N");
        progress.MissionId = mission.Id;
        progress.UserId = userId;
        progress.OrganizationId = organizationId;
        progress.OwnerId = ownerId;
        progress.Status = ModuleConstants.MissionProgressStatuses.InProgress;
        progress.PeriodStart = periodStart;
        progress.PeriodEnd = periodEnd;
        progress.CurrentValue = 0m;
        progress.Percentage = 0m;
        progress.TargetValue = ComputeTargetValue(goal, goalItems);

        if (goal is PerSkuGoal)
        {
            progress.Items = goalItems
                .Select(x => new LoyaltyMissionProgressItem
                {
                    MissionId = mission.Id,
                    MissionProgressId = progress.Id,
                    ProductId = x.ProductId,
                    TargetQuantity = x.Quantity,
                    CurrentQuantity = 0,
                })
                .ToList();
        }

        return progress;
    }

    private static decimal ApplyContribution(LoyaltyMissionProgress progress, IMissionGoal goal, CustomerOrder order)
    {
        switch (goal)
        {
            case OrderValueGoal:
                // Currency mismatch is filtered out earlier in ApplyMissionInternalAsync.
                return order.Total;
            case OrderCountGoal:
                return 1m;
            case PerSkuGoal:
                var added = 0m;
                var itemsByProduct = progress.Items.ToDictionary(x => x.ProductId, StringComparer.OrdinalIgnoreCase);
                foreach (var lineItem in order.Items ?? Enumerable.Empty<LineItem>())
                {
                    if (!lineItem.ProductId.IsNullOrEmpty() && itemsByProduct.TryGetValue(lineItem.ProductId, out var item))
                    {
                        item.CurrentQuantity += lineItem.Quantity;
                        added += lineItem.Quantity;
                    }
                }
                return added;
            default:
                return 0m;
        }
    }

    private static void UpdateMissionProgressMetrics(LoyaltyMissionProgress progress, IMissionGoal goal, decimal contribution)
    {
        if (goal is PerSkuGoal perSku)
        {
            progress.CurrentValue = progress.Items.Sum(i => Math.Min(i.CurrentQuantity, i.TargetQuantity));

            if (perSku.All)
            {
                progress.Percentage = progress.TargetValue > 0 ? Math.Min(100m, progress.CurrentValue / progress.TargetValue * 100m) : 0m;
            }
            else
            {
                var completed = progress.Items.Any(i => i.CurrentQuantity >= i.TargetQuantity);
                progress.Percentage = completed ? 100m : 0m;
            }
        }
        else
        {
            progress.CurrentValue += contribution;
            progress.Percentage = progress.TargetValue > 0 ? Math.Min(100m, progress.CurrentValue / progress.TargetValue * 100m) : 0m;
        }
    }

    private static decimal GetRewardAmount(LoyaltyMissionConditionAndRewardTree tree)
    {
        return tree?.GetLoyaltyRewards()?.Sum(x => x.GetActualRewardAmount(0m)) ?? 0m;
    }

    // PerSku is reported as PerSkuAll / PerSkuAny depending on the goal completion mode.
    private static string ResolveMissionType(IMissionGoal goal)
    {
        return goal is PerSkuGoal perSkuGoal
            ? (perSkuGoal.All ? ModuleConstants.MissionTypes.PerSkuAll : ModuleConstants.MissionTypes.PerSkuAny)
            : goal.MissionType;
    }

    private async Task GrantRewardAsync(LoyaltyMission mission, LoyaltyMissionProgress progress, string userId, string organizationId)
    {
        var amount = GetRewardAmount(mission.DynamicExpression);
        if (amount <= 0)
        {
            return;
        }

        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.ContextObjectType = nameof(LoyaltyMissionProgress);
        context.MissionProgressId = progress.Id;
        context.UserId = userId;
        context.OrganizationId = organizationId;

        var result = AbstractTypeFactory<LoyaltyAmountResult>.TryCreateInstance();
        result.OperationType = ModuleConstants.LoyaltyPrograms.EarnedOperationType;
        result.SourceType = ModuleConstants.LoyaltySourceTypes.LoyaltyMission;
        result.SourceId = mission.Id;
        result.Amount = amount;

        await _loyaltyLogicService.LogLoyaltyProgramOperationAsync(context, result);
    }

    // Deliberately not narrowed by user or organization: the contributing object is an order, whose
    // id is globally unique, so it contributes to a mission exactly once no matter who it is attributed
    // to. Narrowing would also make the gate miss a transaction written under the store's previous
    // balance calculation mode and re-insert it, violating
    // IX_LoyaltyMissionTransaction_MissionId_ObjectId_UserId.
    private async Task<bool> TransactionExistsAsync(string missionId, string objectId)
    {
        var criteria = AbstractTypeFactory<LoyaltyMissionTransactionSearchCriteria>.TryCreateInstance();
        criteria.MissionId = missionId;
        criteria.ObjectId = objectId;
        criteria.Take = 0;

        var result = await _transactionSearchService.SearchNoCloneAsync(criteria);
        return result.TotalCount > 0;
    }

    private async Task<IList<LoyaltyMissionGoalItem>> GetGoalItemsAsync(string missionId)
    {
        var result = new List<LoyaltyMissionGoalItem>();

        var criteria = AbstractTypeFactory<LoyaltyMissionGoalItemSearchCriteria>.TryCreateInstance();
        criteria.MissionId = missionId;
        criteria.Take = 100;

        await foreach (var batch in _goalItemSearchService.SearchBatchesNoCloneAsync(criteria))
        {
            result.AddRange(batch.Results);
        }

        return result;
    }

    private static LoyaltyMissionProgress CreateTransientProgress(LoyaltyMission mission, IMissionGoal goal, string userId, string organizationId, IList<LoyaltyMissionGoalItem> goalItems)
    {
        var (periodStart, periodEnd) = ResolvePeriod(mission);

        var progress = AbstractTypeFactory<LoyaltyMissionProgress>.TryCreateInstance();
        progress.MissionId = mission.Id;
        progress.UserId = userId;
        progress.OrganizationId = organizationId;
        progress.OwnerId = ResolveOwnerId(userId, organizationId);
        progress.Status = ModuleConstants.MissionProgressStatuses.InProgress;
        progress.PeriodStart = periodStart;
        progress.PeriodEnd = periodEnd;
        progress.CurrentValue = 0m;
        progress.Percentage = 0m;
        progress.TargetValue = ComputeTargetValue(goal, goalItems);

        if (goal is PerSkuGoal)
        {
            progress.Items = goalItems
                .Select(x => new LoyaltyMissionProgressItem
                {
                    MissionId = mission.Id,
                    ProductId = x.ProductId,
                    TargetQuantity = x.Quantity,
                    CurrentQuantity = 0,
                })
                .ToList();
        }

        return progress;
    }

    private static decimal ComputeTargetValue(IMissionGoal goal, IList<LoyaltyMissionGoalItem> goalItems)
    {
        return goal switch
        {
            OrderValueGoal orderValue => orderValue.Value,
            OrderCountGoal orderCount => orderCount.Count,
            PerSkuGoal => goalItems.Sum(x => x.Quantity),
            _ => 0m,
        };
    }

    private static IMissionGoal ExtractGoal(LoyaltyMissionConditionAndRewardTree tree)
    {
        return tree?.Traverse<IConditionTree>(x => x.Children ?? []).OfType<IMissionGoal>().FirstOrDefault();
    }

    // The owner a progress record (and its lock) is scoped to: the organization when the store
    // calculates per organization and the order actually has one, the user otherwise.
    private static string ResolveOwnerId(string userId, string organizationId)
    {
        return organizationId.IsNullOrEmpty() ? userId : organizationId;
    }

    private static (DateTime? Start, DateTime? End) ResolvePeriod(LoyaltyMission mission)
    {
        return (mission.StartDate ?? mission.CreatedDate, mission.EndDate);
    }
}
