namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyAmountResult
{
    public string SourceType { get; set; }

    public string SourceId { get; set; }

    public string OperationType { get; set; }

    public decimal Amount { get; set; }
}
