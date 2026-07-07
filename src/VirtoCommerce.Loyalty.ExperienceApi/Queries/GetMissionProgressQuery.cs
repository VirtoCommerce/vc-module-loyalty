using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetMissionProgressQuery : SearchQuery<LoyaltyMissionProgressSearchResult>
{
    public string UserId { get; set; }

    public string Status { get; set; }

    public string StoreId { get; set; }

    public string CultureName { get; set; }

    public override IEnumerable<QueryArgument> GetArguments()
    {
        foreach (var argument in base.GetArguments())
        {
            yield return argument;
        }

        yield return Argument<StringGraphType>(nameof(UserId));
        yield return Argument<StringGraphType>(nameof(Status));
        yield return Argument<StringGraphType>(nameof(StoreId));
        yield return Argument<StringGraphType>(nameof(CultureName));
    }

    public override void Map(IResolveFieldContext context)
    {
        base.Map(context);

        UserId = context.GetArgument<string>(nameof(UserId)) ?? context.GetCurrentUserId();
        Status = context.GetArgument<string>(nameof(Status));
        StoreId = context.GetArgument<string>(nameof(StoreId));
        CultureName = context.GetArgument<string>(nameof(CultureName));
    }
}
