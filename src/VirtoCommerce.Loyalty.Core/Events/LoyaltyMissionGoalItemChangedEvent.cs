using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyMissionGoalItemChangedEvent(IEnumerable<GenericChangedEntry<LoyaltyMissionGoalItem>> changedEntries)
    : GenericChangedEntryEvent<LoyaltyMissionGoalItem>(changedEntries);
