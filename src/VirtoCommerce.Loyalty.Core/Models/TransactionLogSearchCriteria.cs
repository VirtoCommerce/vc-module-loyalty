using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class TransactionLogSearchCriteria : SearchCriteriaBase
{
    public string LoyaltyProgramId { get; set; }

    public string CustomerId { get; set; }

    public LoyaltyOperationType? OperationType { get; set; }
}
