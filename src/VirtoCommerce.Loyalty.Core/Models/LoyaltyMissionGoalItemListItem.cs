namespace VirtoCommerce.Loyalty.Core.Models;

/// <summary>
/// UI projection of <see cref="LoyaltyMissionGoalItem"/> enriched with product display fields.
/// </summary>
public class LoyaltyMissionGoalItemListItem : LoyaltyMissionGoalItem
{
    public string ProductCode { get; set; }

    public string ProductName { get; set; }
}
