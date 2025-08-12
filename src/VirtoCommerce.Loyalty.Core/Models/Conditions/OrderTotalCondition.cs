using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Conditions;

public class OrderTotalCondition : CompareConditionBase
{
    public decimal TotalValue { get; set; }

    public decimal TotalValueSecond { get; set; }

    public override bool IsSatisfiedBy(IEvaluationContext context)
    {
        if (context is not LoyaltyProgramEvaluationContext loyaltyContext)
        {
            return false;
        }

        return UseCompareCondition(loyaltyContext.OrderTotal, TotalValue, TotalValueSecond);
    }
}
