using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyBalanceResult
{
    public decimal CurrentBalance { get; set; }

    public decimal ResultBalance { get; set; }
}

public class LoyaltyBalanceRequest
{
    public string UserId { get; set; }

    public string OrderId { get; set; }

    public CustomerOrder CustomerOrder { get; set; }
}
