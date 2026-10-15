namespace VirtoCommerce.Loyalty.Data.BackgroundJobs;

/// <summary>
/// Payload of <see cref="ProcessLoyaltyOrdersJobHandler"/>: the newly placed orders to award or redeem loyalty points for.
/// </summary>
public class ProcessLoyaltyOrdersJobPayload
{
    public string[] OrderIds { get; set; } = [];
}
