using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Currency;

namespace VirtoCommerce.Loyalty.ExperienceApi.Services;

public class LoyaltyPointsContext
{
    public Currency PointsCurrency { get; init; }
    public decimal DefaultFactor { get; init; }
    public IDictionary<string, decimal> FactorByProductId { get; init; }

    public Money CalculatePoints(decimal price, string productId)
    {
        var factor = FactorByProductId != null && FactorByProductId.TryGetValue(productId, out var value) ? value : DefaultFactor;
        return new Money(price * factor, PointsCurrency);
    }
}
