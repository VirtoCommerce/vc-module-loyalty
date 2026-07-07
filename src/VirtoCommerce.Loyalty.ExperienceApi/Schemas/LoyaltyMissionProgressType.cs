using GraphQL.DataLoader;
using GraphQL.Resolvers;
using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.ExperienceApi.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;

public class LoyaltyMissionProgressType : ExtendableGraphType<LoyaltyMissionProgress>
{
    public LoyaltyMissionProgressType(
        IDataLoaderContextAccessor dataLoader,
        ILoyaltyMissionService missionService)
    {
        Name = "LoyaltyMissionProgress";
        Description = "Represents the progress of a user towards a loyalty mission.";

        Field(x => x.Id).Description("The unique identifier of the progress record.");
        Field(x => x.UserId, nullable: true).Description("The user the progress belongs to.");
        Field(x => x.CurrentValue).Description("The accumulated value towards the mission target.");
        Field(x => x.TargetValue).Description("The mission target value.");
        Field(x => x.Percentage).Description("The completion percentage (0-100).");
        Field(x => x.Status, nullable: true).Description("The progress status (InProgress, Completed, Expired).");
        Field(x => x.CompletedDate, nullable: true).Description("The date and time when the mission was completed.");
        Field(x => x.PeriodStart, nullable: true).Description("The start of the mission occurrence window.");
        Field(x => x.PeriodEnd, nullable: true).Description("The end of the mission occurrence window.");
        Field(x => x.CreatedDate).Description("The date and time when the progress was created.");

        var missionField = new FieldType
        {
            Name = "mission",
            Type = GraphTypeExtensionHelper.GetActualType<LoyaltyMissionType>(),
            Resolver = new FuncFieldResolver<LoyaltyMissionProgress, IDataLoaderResult<LoyaltyMission>>(context =>
                dataLoader.LoadMission(missionService, loaderKey: "loyalty_mission", missionId: context.Source.MissionId))
        };
        AddField(missionField);
    }
}
