using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Currency;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyPointsContext
{
    public Currency PointsCurrency { get; init; }
    public decimal DefaultFactor { get; init; }
    public IDictionary<string, decimal> FactorByProductId { get; init; }

    public Money CalculatePoints(decimal price, string productId)
    {
        if (PointsCurrency == null)
        {
            return null;
        }

        var factor = FactorByProductId != null && FactorByProductId.TryGetValue(productId, out var value) ? value : DefaultFactor;
        return new Money(price * factor, PointsCurrency);
    }
}
