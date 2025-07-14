using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.LoyaltyProgramSearchService.Core.Models;

public class LoyaltyProgramSearchCriteria : SearchCriteriaBase
{
    public string Store { get; set; }
    private string[] _storeIds;
    public string[] StoreIds
    {
        get
        {
            if (_storeIds == null && !string.IsNullOrEmpty(Store))
            {
                _storeIds = [Store];
            }
            return _storeIds;
        }
        set
        {
            _storeIds = value;
        }
    }

    public bool? IsActive { get; set; }
}
