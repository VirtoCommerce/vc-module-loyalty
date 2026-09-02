using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Missions;

/// <summary>
/// Mission goal: purchase the target quantity of the listed SKUs per mission
/// </summary>
public class PerSkuGoal : ConditionTree, IMissionGoal
{
    /// <summary>
    /// Completion mode:
    /// true = All, all listed SKUs must reach its target quantity to complete the mission
    /// false = Any, reaching any single SKU target completes the mission
    /// </summary>
    public bool All { get; set; } = true;

    public string MissionType => ModuleConstants.MissionTypes.PerSku;

    public override bool IsSatisfiedBy(IEvaluationContext context) => true;
}
