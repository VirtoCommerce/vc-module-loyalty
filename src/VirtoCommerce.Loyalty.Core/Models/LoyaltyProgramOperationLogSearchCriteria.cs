using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramOperationLogSearchCriteria : SearchCriteriaBase
{
    public string UserId { get; set; }

    public string ObjectId { get; set; }

    public string LoyaltyProgramId { get; set; }

    public string OperationType { get; set; }
}
