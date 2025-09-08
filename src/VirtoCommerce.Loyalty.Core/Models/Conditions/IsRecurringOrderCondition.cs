using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Conditions;

public class IsRecurringOrderCondition : ConditionTree
{
    public override bool IsSatisfiedBy(IEvaluationContext context)
    {
        if (context is not LoyaltyProgramEvaluationContext loyaltyContext)
        {
            return false;
        }

        return loyaltyContext.IsRecurringOrder;
    }
}

