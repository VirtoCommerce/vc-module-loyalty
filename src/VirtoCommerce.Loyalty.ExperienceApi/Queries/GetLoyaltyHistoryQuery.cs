using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetLoyaltyHistoryQuery : SearchQuery<LoyaltyProgramOperationLogSearchResult>
{
    public string UserId { get; set; }

    public string OperationType { get; set; }

    public override IEnumerable<QueryArgument> GetArguments()
    {
        foreach (var argument in base.GetArguments())
        {
            yield return argument;
        }

        yield return Argument<StringGraphType>(nameof(UserId));
        yield return Argument<StringGraphType>(nameof(OperationType));
    }

    public override void Map(IResolveFieldContext context)
    {
        base.Map(context);

        UserId = context.GetArgument<string>(nameof(UserId)) ?? context.GetCurrentUserId();
        OperationType = context.GetArgument<string>(nameof(OperationType));
    }
}
