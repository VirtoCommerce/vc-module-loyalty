using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyBalanceOperationLogChangingEvent(IEnumerable<GenericChangedEntry<LoyaltyBalanceOperationLog>> changedEntries)
    : GenericChangedEntryEvent<LoyaltyBalanceOperationLog>(changedEntries);
