using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetLoyaltyBalanceQuery : Query<LoyaltyBalanceResult>
{
    public string UserId { get; set; }

    public string OrderId { get; set; }

    public override IEnumerable<QueryArgument> GetArguments()
    {
        yield return Argument<StringGraphType>(nameof(UserId));
        yield return Argument<StringGraphType>(nameof(OrderId));
    }

    public override void Map(IResolveFieldContext context)
    {
        UserId = context.GetArgument<string>(nameof(UserId));
        OrderId = context.GetArgument<string>(nameof(OrderId));
    }
}
