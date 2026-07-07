using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.Loyalty.Core.Models.Conditions;

/// <summary>
/// Goal block of a loyalty mission tree. Holds exactly one mission goal node
/// (order value, order count or SKU-based). A dedicated type so the admin UI can validate it.
/// </summary>
public class BlockLoyaltyMissionGoals : BlockConditionAndOr
{
}
