using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Missions;

/// <summary>
/// Mission goal: purchase the target quantity of the listed SKUs per mission period.
/// Per-SKU items are managed separately as LoyaltyMissionGoalItem records.
/// </summary>
public class PerSkuGoal : ConditionTree, IMissionGoal
{
    /// <summary>
    /// Completion mode:
    /// true = AND (every listed SKU must reach its target quantity),
    /// false = OR (reaching any single SKU target completes the mission).
    /// </summary>
    public bool All { get; set; } = true;

    public string MissionType => ModuleConstants.MissionTypes.PerSku;

    public override bool IsSatisfiedBy(IEvaluationContext context) => true;
}
