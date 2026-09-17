namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public interface ILoyaltyQuery
{
    public string UserId { get; set; }

    public string OrganizationId { get; set; }
}
