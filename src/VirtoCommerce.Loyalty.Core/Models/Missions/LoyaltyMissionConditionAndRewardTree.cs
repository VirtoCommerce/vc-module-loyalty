using System.Linq;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core.Models.Rewards;

namespace VirtoCommerce.Loyalty.Core.Models.Missions;

public class LoyaltyMissionConditionAndRewardTree : BlockConditionAndOr, IHasLoyaltyRewards
{
    public LoyaltyMissionConditionAndRewardTree()
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
