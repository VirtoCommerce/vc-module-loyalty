namespace VirtoCommerce.Loyalty.Data.BackgroundJobs;

/// <summary>
/// Payload of <see cref="ProcessLoyaltyMissionsJobHandler"/>: the newly placed orders to process loyalty missions for.
/// </summary>
public class ProcessLoyaltyMissionsJobPayload
{
    public string[] OrderIds { get; set; } = [];
}
