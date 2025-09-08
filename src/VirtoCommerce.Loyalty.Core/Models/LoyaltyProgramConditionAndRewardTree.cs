using System.Linq;
using VirtoCommerce.CoreModule.Core.Conditions;
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
