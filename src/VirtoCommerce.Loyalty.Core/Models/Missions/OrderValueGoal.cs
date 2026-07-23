using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Missions;

/// <summary>
/// Mission goal: reach the target order total value per mission
/// </summary>
public class OrderValueGoal : ConditionTree, IMissionGoal
{
    public decimal Value { get; set; }

    public string CurrencyCode { get; set; }

    public string MissionType => ModuleConstants.MissionTypes.OrderValue;

    public override bool IsSatisfiedBy(IEvaluationContext context) => true;
}
