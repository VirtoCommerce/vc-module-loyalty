using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyMissionSearchCriteria : SearchCriteriaBase
{
    /// <summary>
    /// Only missions currently running: Status == Published and now within [StartDate, EndDate].
    /// </summary>
    public bool OnlyActive { get; set; }

    public string StoreId { get; set; }

    public string Status { get; set; }

    public bool? Public { get; set; }

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
