using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Rewards;

public class FixedPointsReward : ConditionTree, IHasLoyaltyRewards
{
    public decimal Amount { get; set; }

    public LoyaltyReward[] GetLoyaltyRewards()
    {
        var reward = new LoyaltyReward
        {
            Amount = Amount,
            AmountType = RewardAmountType.Absolute,
        };

        return [reward];
    }
}
