using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyProgramProductFactorChangingEvent(IEnumerable<GenericChangedEntry<LoyaltyProgramProductFactor>> changedEntries)
    : GenericChangedEntryEvent<LoyaltyProgramProductFactor>(changedEntries);
