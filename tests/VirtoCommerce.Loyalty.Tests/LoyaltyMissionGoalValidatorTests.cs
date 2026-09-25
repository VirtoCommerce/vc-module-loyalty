using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Models.Conditions;
using VirtoCommerce.Loyalty.Core.Models.Missions;
using VirtoCommerce.Loyalty.Core.Models.Rewards;
using VirtoCommerce.Loyalty.Data.Validators;
using Xunit;

namespace VirtoCommerce.Loyalty.Tests;

[Trait("Category", "Unit")]
public class LoyaltyMissionGoalValidatorTests
{
    // A goal threshold below zero is satisfied by definition, so the mission would complete and grant
    // its reward without the customer doing anything.
    [Fact]
    public void Validate_OrderCountGoal_NegativeCount_IsInvalid()
    {
        var mission = CreateMission(new OrderCountGoal { Count = -1 });

        var result = new LoyaltyMissionValidator().Validate(mission);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage.Contains("goal") && x.ErrorMessage.Contains("negative"));
    }

    [Fact]
    public void Validate_OrderValueGoal_NegativeValue_IsInvalid()
    {
        var mission = CreateMission(new OrderValueGoal { Value = -1m, CurrencyCode = "USD" });

        var result = new LoyaltyMissionValidator().Validate(mission);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage.Contains("goal") && x.ErrorMessage.Contains("negative"));
    }

    [Fact]
    public void Validate_OrderCountGoal_PositiveCount_IsValid()
    {
        var mission = CreateMission(new OrderCountGoal { Count = 3 });

        var result = new LoyaltyMissionValidator().Validate(mission);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_OrderValueGoal_PositiveValue_IsValid()
    {
        var mission = CreateMission(new OrderValueGoal { Value = 100m, CurrencyCode = "USD" });

        var result = new LoyaltyMissionValidator().Validate(mission);

        Assert.True(result.IsValid);
    }

    // Valid in every other respect, so the goal is the only thing that can fail the mission.
    private static LoyaltyMission CreateMission(IConditionTree goal) => new()
    {
        Name = "Mission",
        StoreId = "store-1",
        DynamicExpression = new LoyaltyMissionConditionAndRewardTree
        {
            Children =
            [
                new BlockLoyaltyMissionCondition { Children = [new AnyUserGroupCondition()] },
                new BlockLoyaltyMissionGoals { Children = [goal] },
                new BlockLoyaltyReward { Children = [new FixedAmountReward { Amount = 10m }] },
            ],
        },
    };
}
