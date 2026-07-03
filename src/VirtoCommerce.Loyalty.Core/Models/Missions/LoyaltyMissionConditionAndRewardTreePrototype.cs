using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core.Models.Conditions;
using VirtoCommerce.Loyalty.Core.Models.Rewards;

namespace VirtoCommerce.Loyalty.Core.Models.Missions;

/// <summary>
/// Prototype for the mission condition-and-reward tree: qualification conditions plus the mission
/// goal nodes in the condition block, and a fixed-amount reward in the reward block.
/// </summary>
public class LoyaltyMissionConditionAndRewardTreePrototype : ConditionTree
{
    public LoyaltyMissionConditionAndRewardTreePrototype()
    {
        IConditionTree[] children =
        [
            new BlockLoyaltyCondition()
                .WithAvailableChildren(
                    new UserGroupsContainsCondition(),
                    new UserGroupIsCondition(),
                    new OrderStatusCondition(),
                    new OrderTotalCondition(),
                    new IsFirstOrderCondition(),
                    new IsRecurringOrderCondition(),
                    new OrderValueGoal(),
                    new OrderCountGoal(),
                    new PerSkuGoal()
                ),
            new BlockLoyaltyReward()
                .WithAvailableChildren(
                    new FixedAmountReward()
                ),
        ];

        WithChildren(children);
        WithAvailableChildren(children);
    }
}
