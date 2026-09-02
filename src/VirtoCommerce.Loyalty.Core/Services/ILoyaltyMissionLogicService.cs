using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyMissionLogicService
{
    /// <summary>
    /// Returns published missions of the store the user qualifies for, each paired with the user's progress.
    /// </summary>
    Task<IList<LoyaltyUserMission>> GetUserMissionsAsync(LoyaltyUserMissionSearchCriteria criteria);

    /// <summary>
    /// Applies an order's contribution to every qualifying active mission of the store.
    /// </summary>
    Task ProcessOrderAsync(CustomerOrder order, Store store);

    /// <summary>
    /// Moves in-progress records of ended missions to the Expired status.
    /// </summary>
    Task ExpireMissionsAsync();
}
