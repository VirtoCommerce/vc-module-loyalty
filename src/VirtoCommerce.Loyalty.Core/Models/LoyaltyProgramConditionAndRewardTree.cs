using System.Linq;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core.Models.Conditions;
using VirtoCommerce.Loyalty.Core.Models.Rewards;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramConditionAndRewardTree : BlockConditionAndOr, IHasLoyaltyRewards
{
    public LoyaltyProgramConditionAndRewardTree()
    {
        All = true;
    }

    public LoyaltyReward[] GetLoyaltyRewards()
    {
        if (Children == null)
        {
            return [];
        }

        return Children.OfType<IHasLoyaltyRewards>().SelectMany(x => x.GetLoyaltyRewards()).ToArray();
    }
}

public class LoyaltyProgramConditionAndRewardTreePrototype : ConditionTree
{
    public LoyaltyProgramConditionAndRewardTreePrototype()
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
                    new IsRegistrationCondition()
                ),
            new BlockLoyaltyReward()
                .WithAvailableChildren(
                    new FixedPointsReward(),
                    new RelativeOrderValueReward()
                ),
        ];

        WithChildren(children);
        WithAvailableChildren(children);
    }
}
