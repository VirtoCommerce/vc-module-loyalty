using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyMissionGoalItemChangingEvent(IEnumerable<GenericChangedEntry<LoyaltyMissionGoalItem>> changedEntries)
    : GenericChangedEntryEvent<LoyaltyMissionGoalItem>(changedEntries);
