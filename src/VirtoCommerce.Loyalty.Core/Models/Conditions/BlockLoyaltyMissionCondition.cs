using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Conditions;

/// <summary>
/// Condition block of a loyalty mission tree. A dedicated type (vs <see cref="BlockLoyaltyCondition"/>)
/// so the admin UI can identify the mission block and validate that it carries exactly one mission goal.
/// </summary>
public class BlockLoyaltyMissionCondition : BlockConditionAndOr
{
}
