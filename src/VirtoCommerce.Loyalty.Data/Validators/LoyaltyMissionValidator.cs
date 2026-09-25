using System.Linq;
using FluentValidation;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Models.Conditions;
using VirtoCommerce.Loyalty.Core.Models.Missions;
using VirtoCommerce.Loyalty.Core.Models.Rewards;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Data.Validators;

public class LoyaltyMissionValidator : AbstractValidator<LoyaltyMission>
{
    public LoyaltyMissionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Mission name is required");
        RuleFor(x => x.StoreId).NotEmpty().WithMessage("Mission store is required");

        RuleFor(x => x).Custom((mission, context) =>
        {
            var nodes = mission.DynamicExpression?.Traverse<IConditionTree>(x => x.Children ?? []).ToArray() ?? [];

            var conditionsBlock = nodes.OfType<BlockLoyaltyMissionCondition>().FirstOrDefault();
            if (conditionsBlock?.Children.IsNullOrEmpty() != false)
            {
                context.AddFailure(nameof(LoyaltyMission.DynamicExpression), "Mission must have at least one condition");
            }

            var goals = nodes.OfType<IMissionGoal>().ToArray();
            if (goals.Length != 1)
            {
                context.AddFailure(nameof(LoyaltyMission.DynamicExpression), "Mission must have exactly one goal");
            }

            var rewardBlock = nodes.OfType<BlockLoyaltyReward>().FirstOrDefault();
            var rewards = rewardBlock?.GetLoyaltyRewards();
            if (rewards.IsNullOrEmpty())
            {
                context.AddFailure(nameof(LoyaltyMission.DynamicExpression), "Mission must have at least one reward");
            }
            else if (rewards.Any(x => x.Amount < 0))
            {
                context.AddFailure(nameof(LoyaltyMission.DynamicExpression), "Mission reward amount cannot be negative");
            }

            if (goals.Length == 1 && goals[0] is OrderValueGoal { CurrencyCode: null or "" })
            {
                context.AddFailure(nameof(LoyaltyMission.DynamicExpression), "Currency code is required for the order value goal");
            }

            if (goals.Any(x => x is OrderCountGoal { Count: < 0 } or OrderValueGoal { Value: < 0 }))
            {
                context.AddFailure(nameof(LoyaltyMission.DynamicExpression), "Mission goal value cannot be negative");
            }
        });
    }
}
