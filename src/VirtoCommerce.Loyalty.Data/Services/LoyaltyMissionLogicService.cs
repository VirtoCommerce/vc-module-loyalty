using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.CoreModule.Core.Currency;
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
    private readonly ILoyaltyMissionTransactionService _transactionService;
    private readonly ILoyaltyMissionTransactionSearchService _transactionSearchService;
    private readonly ILoyaltyLogicService _loyaltyLogicService;
    private readonly IDistributedLockService _distributedLockService;
    private readonly IStoreService _storeService;
    private readonly ICurrencyService _currencyService;

    public LoyaltyMissionLogicService(
        ILoyaltyMissionSearchService missionSearchService,
        ILoyaltyMissionGoalItemSearchService goalItemSearchService,
        ILoyaltyMissionProgressService progressService,
        ILoyaltyMissionProgressSearchService progressSearchService,
        ILoyaltyMissionTransactionService transactionService,
        ILoyaltyMissionTransactionSearchService transactionSearchService,
        ILoyaltyLogicService loyaltyLogicService,
        IDistributedLockService distributedLockService,
        IStoreService storeService,
        ICurrencyService currencyService)
    {
        _missionSearchService = missionSearchService;
        _goalItemSearchService = goalItemSearchService;
        _progressService = progressService;
        _progressSearchService = progressSearchService;
        _transactionService = transactionService;
        _transactionSearchService = transactionSearchService;
        _loyaltyLogicService = loyaltyLogicService;
        _distributedLockService = distributedLockService;
        _storeService = storeService;
        _currencyService = currencyService;
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

        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.ContextObjectType = nameof(CustomerOrder);
        context.OrderId = order.Id;
        await _loyaltyLogicService.PopulateLoyaltyProgramEvaluationContextAsync(context);

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

                await ApplyMissionAsync(mission, goal, order, userId);
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

    public async Task<IList<LoyaltyUserMission>> GetUserMissionsAsync(string userId, string storeId, IList<string> statuses, DateTime? completedStartDate = null, DateTime? completedEndDate = null, bool? isStarted = null)
    {
        if (storeId.IsNullOrEmpty() || userId.IsNullOrEmpty())
        {
            return [];
        }

        // 1. Published missions of the store the user qualifies for.
        var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
        context.ContextObjectType = nameof(ApplicationUser);
        context.UserId = userId;
        context.StoreId = storeId;
        await _loyaltyLogicService.PopulateLoyaltyProgramEvaluationContextAsync(context);

        var missionCriteria = AbstractTypeFactory<LoyaltyMissionSearchCriteria>.TryCreateInstance();
        missionCriteria.StoreIds = [storeId];
        missionCriteria.OnlyActive = true;
        missionCriteria.Take = 50;

        var qualifyingMissions = new List<LoyaltyMission>();
        await foreach (var batch in _missionSearchService.SearchBatchesNoCloneAsync(missionCriteria))
        {
            qualifyingMissions.AddRange(batch.Results
                .Where(x => x.DynamicExpression?.IsSatisfiedBy(context) ?? false));
        }

        if (qualifyingMissions.Count == 0)
        {
            return [];
        }

        // 2. Progress records for the qualifying missions (all statuses; the status filter is applied on the result).
        var progressByMissionId = new Dictionary<string, LoyaltyMissionProgress>(StringComparer.OrdinalIgnoreCase);
        var progressCriteria = AbstractTypeFactory<LoyaltyMissionProgressSearchCriteria>.TryCreateInstance();
        progressCriteria.UserId = userId;
        progressCriteria.MissionIds = qualifyingMissions.Select(x => x.Id).ToArray();
        progressCriteria.Take = 100;

        await foreach (var batch in _progressSearchService.SearchBatchesNoCloneAsync(progressCriteria))
        {
            foreach (var progress in batch.Results)
            {
                progressByMissionId.TryAdd(progress.MissionId, progress);
            }
        }

        // 3. Resolve the store currencies used to format the money values (once for all missions).
        var store = await _storeService.GetNoCloneAsync(storeId);
        var currencies = await _currencyService.GetAllCurrenciesAsync();
        var mainCurrency = currencies.FirstOrDefault(x => x.Code.EqualsIgnoreCase(store?.DefaultCurrency));
        var pointsCurrencyCode = store?.GetLoyaltyCurrencyCode();
        var pointsCurrency = pointsCurrencyCode.IsNullOrEmpty()
            ? null
            : currencies.FirstOrDefault(x => x.Code.EqualsIgnoreCase(pointsCurrencyCode));

        // 4. Pair every qualifying mission with its progress (real or transient 0%).
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
                var goalItems = goal is PerSkuGoal ? await GetGoalItemsAsync(mission.Id) : [];
                progress = CreateTransientProgress(mission, goal, userId, goalItems);
            }

            result.Add(new LoyaltyUserMission
            {
                Mission = mission,
                Progress = progress,
                Store = store,
                MissionType = ResolveMissionType(goal),
                RewardPoints = GetRewardAmount(mission.DynamicExpression),
                MissionCurrency = mainCurrency,
                PointsCurrency = pointsCurrency,
            });
        }

        // 5. Apply the requested progress-status filter.
        if (!statuses.IsNullOrEmpty())
        {
            result = result.Where(x => statuses.Contains(x.Progress.Status)).ToList();
        }

        // 6. Apply the CompletedDate range filter (keeps only missions completed within the range).
        if (completedStartDate != null)
        {
            result = result.Where(x => x.Progress.CompletedDate != null && x.Progress.CompletedDate >= completedStartDate).ToList();
        }

        if (completedEndDate != null)
        {
            result = result.Where(x => x.Progress.CompletedDate != null && x.Progress.CompletedDate <= completedEndDate).ToList();
        }

        // 7. Apply the started/not-started filter (started = a real progress record exists).
        if (isStarted != null)
        {
            result = result.Where(x => !string.IsNullOrEmpty(x.Progress.Id) == isStarted.Value).ToList();
        }

        return result;
    }

    private Task ApplyMissionAsync(LoyaltyMission mission, IMissionGoal goal, CustomerOrder order, string userId)
    {
        // Serialize the per-user/mission progress read-modify-write. The reward credit uses a
        // separate per-user balance lock (different resource key) inside the loyalty logic service.
        return _distributedLockService.ExecuteAsync($"loyalty-mission:{mission.Id}:{userId}",
            () => ApplyMissionInternalAsync(mission, goal, order, userId),
            lockTimeout: TimeSpan.FromSeconds(30),
            tryLockTimeout: TimeSpan.FromSeconds(30),
            retryInterval: TimeSpan.FromMilliseconds(200));
    }

    private async Task<bool> ApplyMissionInternalAsync(LoyaltyMission mission, IMissionGoal goal, CustomerOrder order, string userId)
    {
        // Idempotency: an order contributes to a mission once.
        if (await TransactionExistsAsync(mission.Id, order.Id, userId))
        {
            return false;
        }

        var goalItems = goal is PerSkuGoal ? await GetGoalItemsAsync(mission.Id) : [];

        var eventDate = order.CreatedDate == default ? DateTime.UtcNow : order.CreatedDate;
        var progress = await GetOrCreateProgressAsync(mission, goal, userId, eventDate, goalItems);

        // An already completed mission no longer accumulates: skip logging the transaction and updating the progress.
        if (progress.Status.EqualsIgnoreCase(ModuleConstants.MissionProgressStatuses.Completed))
        {
            return false;
        }

        var contribution = ApplyContribution(progress, goal, order);
        UpdateMetrics(progress, goal, contribution, out var completed);

        if (completed)
        {
            progress.Status = ModuleConstants.MissionProgressStatuses.Completed;
            progress.CompletedDate = DateTime.UtcNow;
        }

        // Log the transaction first as the idempotency gate, then persist the progress.
        var transaction = AbstractTypeFactory<LoyaltyMissionTransaction>.TryCreateInstance();
        transaction.Id = Guid.NewGuid().ToString("N");
        transaction.MissionId = mission.Id;
        transaction.MissionProgressId = progress.Id;
        transaction.UserId = userId;
        transaction.ObjectId = order.Id;
        transaction.ObjectType = nameof(CustomerOrder);
        transaction.ContributionValue = contribution;
        await _transactionService.SaveChangesAsync([transaction]);

        await _progressService.SaveChangesAsync([progress]);

        if (completed)
        {
            await GrantRewardAsync(mission, progress, userId);
        }

        return true;
    }

    private async Task<LoyaltyMissionProgress> GetOrCreateProgressAsync(
        LoyaltyMission mission,
        IMissionGoal goal,
        string userId,
        DateTime eventDate,
        IList<LoyaltyMissionGoalItem> goalItems)
    {
        var (periodStart, periodEnd) = ResolvePeriod(mission, eventDate);

        var criteria = AbstractTypeFactory<LoyaltyMissionProgressSearchCriteria>.TryCreateInstance();
        criteria.MissionId = mission.Id;
        criteria.UserId = userId;
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

    private static void UpdateMetrics(LoyaltyMissionProgress progress, IMissionGoal goal, decimal contribution, out bool completed)
    {
        if (goal is PerSkuGoal perSku)
        {
            progress.CurrentValue = progress.Items.Sum(i => Math.Min(i.CurrentQuantity, i.TargetQuantity));

            completed = perSku.All
                ? progress.Items.Count > 0 && progress.Items.All(i => i.CurrentQuantity >= i.TargetQuantity)
                : progress.Items.Any(i => i.CurrentQuantity >= i.TargetQuantity);

            progress.Percentage = perSku.All
                ? (progress.TargetValue > 0 ? Math.Min(100m, progress.CurrentValue / progress.TargetValue * 100m) : 0m)
                : (completed ? 100m : 0m);
        }
        else
        {
            progress.CurrentValue += contribution;
            completed = progress.TargetValue > 0 && progress.CurrentValue >= progress.TargetValue;
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

    private async Task GrantRewardAsync(LoyaltyMission mission, LoyaltyMissionProgress progress, string userId)
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

        var result = AbstractTypeFactory<LoyaltyAmountResult>.TryCreateInstance();
        result.OperationType = ModuleConstants.LoyaltyPrograms.EarnedOperationType;
        result.SourceType = ModuleConstants.LoyaltySourceTypes.LoyaltyMission;
        result.SourceId = mission.Id;
        result.Amount = amount;

        await _loyaltyLogicService.LogLoyaltyProgramOperationAsync(context, result);
    }

    private async Task<bool> TransactionExistsAsync(string missionId, string objectId, string userId)
    {
        var criteria = AbstractTypeFactory<LoyaltyMissionTransactionSearchCriteria>.TryCreateInstance();
        criteria.MissionId = missionId;
        criteria.ObjectId = objectId;
        criteria.UserId = userId;
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

    private static LoyaltyMissionProgress CreateTransientProgress(LoyaltyMission mission, IMissionGoal goal, string userId, IList<LoyaltyMissionGoalItem> goalItems)
    {
        var (periodStart, periodEnd) = ResolvePeriod(mission, DateTime.UtcNow);

        var progress = AbstractTypeFactory<LoyaltyMissionProgress>.TryCreateInstance();
        progress.MissionId = mission.Id;
        progress.UserId = userId;
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

    // Reset periodicity is a future enabler. Only "None" is processed: the whole mission window.
    // PeriodStart is kept non-null (falls back to the mission creation date) so the unique
    // (MissionId, UserId, PeriodStart) index always applies.
    private static (DateTime? Start, DateTime? End) ResolvePeriod(LoyaltyMission mission, DateTime eventDate)
    {
        return (mission.StartDate ?? mission.CreatedDate, mission.EndDate);
    }
}
