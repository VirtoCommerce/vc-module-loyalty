using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;

public class LoyaltyMissionType : ExtendableGraphType<LoyaltyMission>
{
    public LoyaltyMissionType()
    {
        Name = "LoyaltyMission";
        Description = "Represents a loyalty mission definition.";

        Field(x => x.Id).Description("The unique identifier of the mission.");
        Field(x => x.Name, nullable: true).Description("The internal name of the mission.");
        Field(x => x.Status, nullable: true).Description("The mission status (Draft, Published, Archived).");

        Field<StringGraphType>("localizedName")
            .Description("The localized display name of the mission.")
            .Resolve(context =>
            {
                var cultureName = context.GetArgumentOrValue<string>("cultureName");
                return context.Source.LocalizedName?.GetValue(cultureName) ?? context.Source.Name;
            });

        Field<StringGraphType>("description")
            .Description("The localized description of the mission.")
            .Resolve(context =>
            {
                var cultureName = context.GetArgumentOrValue<string>("cultureName");
                return context.Source.Description?.GetValue(cultureName);
            });
    }
}
