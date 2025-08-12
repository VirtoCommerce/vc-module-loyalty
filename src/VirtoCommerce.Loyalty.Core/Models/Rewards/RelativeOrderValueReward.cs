using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Rewards;

public class RelativeOrderValueReward : ConditionTree
{
    public decimal Amount { get; set; }
}
