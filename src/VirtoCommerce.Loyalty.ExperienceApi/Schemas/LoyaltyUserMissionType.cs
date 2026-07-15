using System;
using GraphQL.Types;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;

/// <summary>
/// A flat union of the mission definition and the current user's progress on it.
/// </summary>
public class LoyaltyUserMissionType : ExtendableGraphType<LoyaltyUserMission>
{
    public LoyaltyUserMissionType()
    {
        Name = "LoyaltyUserMission";
        Description = "Represents a loyalty mission together with the current user's progress.";

        // Mission fields
        Field<StringGraphType>("missionId")
            .Description("The mission identifier.")
            .Resolve(context => context.Source.Mission?.Id);

        Field<StringGraphType>("name")
            .Description("The internal mission name.")
            .Resolve(context => context.Source.Mission?.Name);

        Field<StringGraphType>("localizedName")
            .Description("The localized mission name.")
            .Resolve(context =>
            {
                var cultureName = context.GetArgumentOrValue<string>("cultureName");
                return context.Source.Mission?.LocalizedName?.GetValue(cultureName) ?? context.Source.Mission?.Name;
            });

        Field<StringGraphType>("description")
            .Description("The localized mission description.")
            .Resolve(context =>
            {
                var cultureName = context.GetArgumentOrValue<string>("cultureName");
                return context.Source.Mission?.Description?.GetValue(cultureName);
            });

        Field<StringGraphType>("bannerUrl")
            .Description("The mission banner image URL.")
            .Resolve(context => context.Source.Mission?.BannerUrl);

        Field<DateTimeGraphType>("startDate")
            .Description("The mission start date.")
            .Resolve(context => context.Source.Mission?.StartDate);

        Field<DateTimeGraphType>("endDate")
            .Description("The mission end date.")
            .Resolve(context => context.Source.Mission?.EndDate);

        Field<StringGraphType>("missionType")
            .Description("The mission type: OrderValue, OrderCount or PerSku.")
            .Resolve(context => context.Source.MissionType);

        Field<MoneyType>("rewardPoints")
            .Description("The loyalty points granted on completion.")
            .Resolve(context => context.Source.PointsCurrency == null
                ? null
                : new Money(context.Source.RewardPoints, context.Source.PointsCurrency));

        Field<CurrencyType>("missionCurrency")
            .Description("The store main currency used to format the target/current money values.")
            .Resolve(context => context.Source.MissionCurrency);

        Field<IntGraphType>("daysRemaining")
            .Description("Whole days left until the mission ends. Null when the mission has no end date.")
            .Resolve(context =>
            {
                var endDate = context.Source.Mission?.EndDate;
                if (endDate == null)
                {
                    return null;
                }

                var days = (int)Math.Ceiling((endDate.Value - DateTime.UtcNow).TotalDays);
                return days < 0 ? 0 : days;
            });

        // Progress fields
        Field<StringGraphType>("progressId")
            .Description("The progress identifier. Null when the user has not started the mission yet.")
            .Resolve(context => context.Source.Progress?.Id);

        Field<BooleanGraphType>("isStarted")
            .Description("Whether the user has started the mission.")
            .Resolve(context => !string.IsNullOrEmpty(context.Source.Progress?.Id));

        Field<StringGraphType>("status")
            .Description("The progress status (InProgress, Completed, Expired).")
            .Resolve(context => context.Source.Progress?.Status);

        Field<DecimalGraphType>("currentValue")
            .Description("The accumulated value towards the mission target.")
            .Resolve(context => context.Source.Progress?.CurrentValue ?? 0m);

        Field<DecimalGraphType>("targetValue")
            .Description("The mission target value.")
            .Resolve(context => context.Source.Progress?.TargetValue ?? 0m);

        Field<DecimalGraphType>("percentage")
            .Description("The completion percentage (0-100).")
            .Resolve(context => context.Source.Progress?.Percentage ?? 0m);

        Field<DateTimeGraphType>("completedDate")
            .Description("The date and time when the mission was completed.")
            .Resolve(context => context.Source.Progress?.CompletedDate);

        Field<DateTimeGraphType>("periodStart")
            .Description("The start of the mission occurrence window.")
            .Resolve(context => context.Source.Progress?.PeriodStart);

        Field<DateTimeGraphType>("periodEnd")
            .Description("The end of the mission occurrence window.")
            .Resolve(context => context.Source.Progress?.PeriodEnd);

        Field<ListGraphType<LoyaltyMissionProgressItemType>>("items")
            .Description("Per-SKU progress items for PerSku missions.")
            .Resolve(context => context.Source.Progress?.Items);
    }
}
