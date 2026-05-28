using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramSearchCriteria : SearchCriteriaBase
{
    public bool OnlyActive { get; set; }

    public string StoreId { get; set; }

    public string ProgramType { get; set; }

    private string[] _storeIds;
    public string[] StoreIds
    {
        get
        {
            if (_storeIds.IsNullOrEmpty() && !string.IsNullOrEmpty(StoreId))
            {
                _storeIds = [StoreId];
            }
            return _storeIds;
        }
        set
        {
            _storeIds = value;
        }
    }
}
