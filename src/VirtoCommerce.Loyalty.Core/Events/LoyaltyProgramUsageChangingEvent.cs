using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyProgramUsageChangingEvent(IEnumerable<GenericChangedEntry<LoyaltyProgramUsage>> changedEntries)
    : GenericChangedEntryEvent<LoyaltyProgramUsage>(changedEntries);
