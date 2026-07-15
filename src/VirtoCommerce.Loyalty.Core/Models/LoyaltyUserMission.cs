using VirtoCommerce.CoreModule.Core.Currency;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyUserMission
{
    public LoyaltyMission Mission { get; set; }

    public LoyaltyMissionProgress Progress { get; set; }

    /// <summary>
    /// Mission type derived from the goal node: OrderValue / OrderCount / PerSku.
    /// </summary>
    public string MissionType { get; set; }

    /// <summary>
    /// Reward amount (loyalty points) granted on completion.
    /// </summary>
    public decimal RewardPoints { get; set; }

    /// <summary>
    /// Store main currency used to format the money-based target/current values.
    /// </summary>
    public Currency MissionCurrency { get; set; }

    /// <summary>
    /// Loyalty points currency used to format.
    /// </summary>
    public Currency PointsCurrency { get; set; }
}
