using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyBalanceOperationLogSearchCriteria : SearchCriteriaBase
{
    public string UserId { get; set; }

    public string ObjectId { get; set; }

    public string SourceType { get; set; }

    public string SourceId { get; set; }

    public string OperationType { get; set; }
}
