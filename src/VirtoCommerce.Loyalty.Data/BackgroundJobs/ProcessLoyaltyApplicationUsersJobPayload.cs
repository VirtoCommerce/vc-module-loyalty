namespace VirtoCommerce.Loyalty.Data.BackgroundJobs;

/// <summary>
/// Payload of <see cref="ProcessLoyaltyApplicationUsersJobHandler"/>: the newly created users to award registration
/// loyalty points to.
/// </summary>
public class ProcessLoyaltyApplicationUsersJobPayload
{
    public string[] ApplicationUserIds { get; set; } = [];
}
