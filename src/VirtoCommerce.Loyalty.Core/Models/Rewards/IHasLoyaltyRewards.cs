namespace VirtoCommerce.Loyalty.Core.Models.Rewards;
public interface IHasLoyaltyRewards
{
    public LoyaltyReward[] GetLoyaltyRewards();
}
