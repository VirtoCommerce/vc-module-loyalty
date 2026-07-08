using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Missions;

/// <summary>
/// Mission goal: place the target number of orders per mission period.
/// </summary>
public class OrderCountGoal : ConditionTree, IMissionGoal
{
    public int Count { get; set; }

    public string MissionType => ModuleConstants.MissionTypes.OrderCount;

    public override bool IsSatisfiedBy(IEvaluationContext context) => true;
}
