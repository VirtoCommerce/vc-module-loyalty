using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Rewards;

public class FixedPointsReward : ConditionTree
{
    public decimal Amount { get; set; }
}
