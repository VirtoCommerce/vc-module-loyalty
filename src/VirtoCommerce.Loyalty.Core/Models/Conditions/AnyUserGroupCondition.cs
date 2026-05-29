using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Conditions;

public class AnyUserGroupCondition : ConditionTree
{
    public override bool IsSatisfiedBy(IEvaluationContext context)
    {
        return context is LoyaltyProgramEvaluationContext;
    }
}
