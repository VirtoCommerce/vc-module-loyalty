using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramEvaluationContext : EvaluationContextBase, ICacheKey
{
    public string ContextObjectType { get; set; }

    public string ContextObjectId
    {
        get
        {
            return ContextObjectType switch
            {
                nameof(CustomerOrder) => OrderId,
                nameof(ApplicationUser) => UserId,
                _ => null,
            };
        }
    }

    public string ProgramType { get; set; } = ModuleConstants.LoyaltyPrograms.DefaultProgramType;

    public string UserId { get; set; }
    public string StoreId { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsFirstOrder { get; set; }
    public bool IsRegistration { get; set; }
    public bool IsRecurringOrder { get; set; }
    public string OrderId { get; set; }
    public string OrderStatus { get; set; }
    public decimal OrderTotal { get; set; }

    public string GetCacheKey()
    {
        var keyValues = GetCacheKeyComponents()
            .Select(x => x is string ? $"'{x}'" : x)
            .Select(x => x is ICacheKey cacheKey ? cacheKey.GetCacheKey() : x?.ToString());

        return string.Join("|", keyValues);
    }

    public virtual IEnumerable<object> GetCacheKeyComponents()
    {
        yield return StoreId;
        yield return CurrencyCode;
        yield return UserId;
        yield return IsFirstOrder;
        yield return IsRegistration;
        yield return IsRecurringOrder;
        yield return OrderId;
        yield return OrderStatus;
        yield return OrderTotal;

        yield return Language;

        foreach (var entry in GetCollectionComponents(UserGroups))
        {
            yield return entry;
        }

        foreach (var entry in GetCollectionComponents(Attributes))
        {
            yield return entry;
        }
    }

    protected virtual IEnumerable<object> GetCollectionComponents<T>(IEnumerable<T> entries)
    {
        if (entries == null)
        {
            yield return null;
        }
        else
        {
            yield return '[';

            foreach (var entry in entries)
            {
                yield return entry;
            }

            yield return ']';
        }
    }
}
