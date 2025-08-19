using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Rewards;

public class RelativeAmountReward : ConditionTree, IHasLoyaltyRewards
{
    public decimal Amount { get; set; }

    public LoyaltyReward[] GetLoyaltyRewards()
    {
        var reward = new LoyaltyReward
        {
            Amount = Amount,
            AmountType = RewardAmountType.Relative,
        };

        return [reward];
    }
}
