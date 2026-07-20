using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetMissionProgressQuery : SearchQuery<LoyaltyUserMissionSearchResult>
{
    public string UserId { get; set; }

    public IList<string> Statuses { get; set; }

    public string StoreId { get; set; }

    public string CultureName { get; set; }

    /// <summary>
    /// Optional lower bound for the mission CompletedDate filter.
    /// </summary>
    public DateTime? CompletedStartDate { get; set; }

    /// <summary>
    /// Optional upper bound for the mission CompletedDate filter.
    /// </summary>
    public DateTime? CompletedEndDate { get; set; }

    /// <summary>
    /// Optional filter by whether the user has started the mission (true = started, false = not started yet).
    /// </summary>
    public bool? IsStarted { get; set; }

    public override IEnumerable<QueryArgument> GetArguments()
    {
        foreach (var argument in base.GetArguments())
        {
            yield return argument;
        }

        yield return Argument<StringGraphType>(nameof(UserId));
        yield return Argument<ListGraphType<StringGraphType>>(nameof(Statuses));
        yield return Argument<StringGraphType>(nameof(StoreId));
        yield return Argument<StringGraphType>(nameof(CultureName));
        yield return Argument<DateTimeGraphType>(nameof(CompletedStartDate));
        yield return Argument<DateTimeGraphType>(nameof(CompletedEndDate));
        yield return Argument<BooleanGraphType>(nameof(IsStarted));
    }

    public override void Map(IResolveFieldContext context)
    {
        base.Map(context);

        UserId = context.GetArgument<string>(nameof(UserId)) ?? context.GetCurrentUserId();
        Statuses = context.GetArgument<IList<string>>(nameof(Statuses));
        StoreId = context.GetArgument<string>(nameof(StoreId));
        CultureName = context.GetArgument<string>(nameof(CultureName));
        CompletedStartDate = context.GetArgument<DateTime?>(nameof(CompletedStartDate));
        CompletedEndDate = context.GetArgument<DateTime?>(nameof(CompletedEndDate));
        IsStarted = context.GetArgument<bool?>(nameof(IsStarted));
    }
}
