using System.Linq;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Rewards;

public class BlockLoyaltyReward : ConditionTree, IHasLoyaltyRewards
{
    public LoyaltyReward[] GetLoyaltyRewards()
    {
        if (Children == null)
        {
            return null;
        }

        return Children.OfType<IHasLoyaltyRewards>().SelectMany(x => x.GetLoyaltyRewards()).ToArray();
    }
}
