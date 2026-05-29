namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyStoreSetting
{
    public string StoreId { get; set; }

    public bool LoyaltyEnabled { get; set; }

    public string LoyaltyMode { get; set; }

    public string LoyaltyCurrency { get; set; }
}
