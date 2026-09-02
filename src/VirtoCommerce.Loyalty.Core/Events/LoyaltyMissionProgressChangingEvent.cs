using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyMissionProgressChangingEvent(IEnumerable<GenericChangedEntry<LoyaltyMissionProgress>> changedEntries)
    : GenericChangedEntryEvent<LoyaltyMissionProgress>(changedEntries);
