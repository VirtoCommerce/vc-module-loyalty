using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyMissionChangingEvent(IEnumerable<GenericChangedEntry<LoyaltyMission>> changedEntries)
    : GenericChangedEntryEvent<LoyaltyMission>(changedEntries);
