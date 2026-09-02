using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetLoyaltyBalanceQuery : Query<LoyaltyBalanceResult>, ILoyaltyQuery
{
    public string UserId { get; set; }

    public string OrganizationId { get; set; }

    public string OrderId { get; set; }


    public override IEnumerable<QueryArgument> GetArguments()
    {
        yield return Argument<StringGraphType>(nameof(UserId));
        yield return Argument<StringGraphType>(nameof(OrderId));
        yield return Argument<StringGraphType>(nameof(OrganizationId));
    }

    public override void Map(IResolveFieldContext context)
    {
        UserId = context.GetArgument<string>(nameof(UserId)) ?? context.GetCurrentUserId();
        OrderId = context.GetArgument<string>(nameof(OrderId));
        OrganizationId = context.GetArgument<string>(nameof(OrganizationId));
    }
}
