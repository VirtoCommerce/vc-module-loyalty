using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Models.Conditions;
using VirtoCommerce.Loyalty.Core.Models.Missions;
using VirtoCommerce.Loyalty.Core.Models.Rewards;
using VirtoCommerce.Loyalty.Data.Validators;
using Xunit;

namespace VirtoCommerce.Loyalty.Tests;

[Trait("Category", "Unit")]
public class LoyaltyMissionValidatorTests
{
    // A negative reward would grant a negative point balance on completion. This platform has no
    // balance-write API and no reversal path, so a landed negative grant is uncorrectable.
    [Fact]
    public void Validate_FixedAmountReward_NegativeAmount_IsInvalid()
    {
        var mission = CreateMission(new FixedAmountReward { Amount = -1m });

        var result = new LoyaltyMissionValidator().Validate(mission);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage.Contains("negative"));
    }

    // Same defect reached through the other reward type - both project Amount onto LoyaltyReward.Amount.
    [Fact]
    public void Validate_RelativeAmountReward_NegativeAmount_IsInvalid()
    {
        var mission = CreateMission(new RelativeAmountReward { Amount = -1m });

        var result = new LoyaltyMissionValidator().Validate(mission);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage.Contains("negative"));
    }

    // Boundary that must not regress: a zero-reward mission is legitimate.
    [Fact]
    public void Validate_FixedAmountReward_ZeroAmount_IsValid()
    {
        var mission = CreateMission(new FixedAmountReward { Amount = 0m });

        var result = new LoyaltyMissionValidator().Validate(mission);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_FixedAmountReward_PositiveAmount_IsValid()
    {
        var mission = CreateMission(new FixedAmountReward { Amount = 100m });

        var result = new LoyaltyMissionValidator().Validate(mission);

        Assert.True(result.IsValid);
    }

    // Valid in every other respect, so the reward amount is the only thing that can fail the mission.
    private static LoyaltyMission CreateMission(IConditionTree reward) => new()
    {
        Name = "Mission",
        StoreId = "store-1",
        DynamicExpression = new LoyaltyMissionConditionAndRewardTree
        {
            Children =
            [
                new BlockLoyaltyMissionCondition { Children = [new AnyUserGroupCondition()] },
                new BlockLoyaltyMissionGoals { Children = [new OrderCountGoal { Count = 1 }] },
                new BlockLoyaltyReward { Children = [reward] },
            ],
        },
    };
}
