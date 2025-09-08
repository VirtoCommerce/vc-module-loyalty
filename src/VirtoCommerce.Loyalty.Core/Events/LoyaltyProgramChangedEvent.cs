using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyProgramChangedEvent(IEnumerable<GenericChangedEntry<LoyaltyProgram>> changedEntries)
    : GenericChangedEntryEvent<LoyaltyProgram>(changedEntries);
