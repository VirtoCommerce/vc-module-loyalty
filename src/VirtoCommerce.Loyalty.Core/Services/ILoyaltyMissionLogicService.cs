using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyMissionLogicService
{
    /// <summary>
    /// Applies an order's contribution to every qualifying active mission of the store:
    /// logs a transaction, updates the per-user/period progress and grants the reward on completion.
    /// </summary>
    Task ProcessOrderAsync(CustomerOrder order, Store store);

    /// <summary>
    /// Moves in-progress records of ended missions to the Expired status.
    /// </summary>
    Task ExpireMissionsAsync();
}
