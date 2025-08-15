using VirtoCommerce.Loyalty.Core.Models.Rewards;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramsEvaluationResult
{
    public LoyaltyReward Reward { get; set; }

    public decimal ActualRewardAmount { get; set; }
}

