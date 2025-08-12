using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models.Conditions;

public class OrderStatusCondition : ConditionTree
{
    public string[] OrderStatuses { get; set; }

    public override bool IsSatisfiedBy(IEvaluationContext context)
    {
        if (context is not LoyaltyProgramEvaluationContext loyaltyContext || OrderStatuses.IsNullOrEmpty())
        {
            return false;
        }

        return OrderStatuses.ContainsIgnoreCase(loyaltyContext.OrderStatus);
    }
}
