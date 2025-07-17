using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyProgramChangingEvent(IEnumerable<GenericChangedEntry<LoyaltyProgram>> changedEntries)
    : GenericChangedEntryEvent<LoyaltyProgram>(changedEntries);
