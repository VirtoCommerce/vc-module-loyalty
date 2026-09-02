using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.Loyalty.Core.Models;

/// <summary>
/// Union of the mission definition and the user's progress on it
/// </summary>
public class LoyaltyUserMission
{
    public LoyaltyMission Mission { get; set; }

    public LoyaltyMissionProgress Progress { get; set; }

    public Store Store { get; set; }

    /// <summary>
    /// Mission type: OrderValue / OrderCount / PerSkuAny / PerSkuAll
    /// </summary>
    public string MissionType { get; set; }

    public decimal RewardPoints { get; set; }

    // Hold currenies as string codes because we need to format them based on the culture in GraphQl request
    public string MissionCurrencyCode { get; set; }
    public string PointsCurrencyCode { get; set; }
}
