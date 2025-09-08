using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models.Rewards;

public class LoyaltyReward : ValueObject
{
    public LoyaltyProgram LoyaltyProgram { get; set; }

    public RewardAmountType AmountType { get; set; }

    public decimal Amount { get; set; }

    public virtual decimal GetActualRewardAmount(decimal total)
    {
        if (total < 0)
        {
            throw new ArgumentException($"The {nameof(total)} cannot be negative", nameof(total));
        }

        var totalAmount = Amount;

        if (AmountType == RewardAmountType.Relative)
        {
            totalAmount = totalAmount * total * 0.01m;
        }

        return totalAmount;
    }
}
