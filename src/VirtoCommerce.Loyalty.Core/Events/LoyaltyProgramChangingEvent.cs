using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyProgramChangingEvent : GenericChangedEntryEvent<LoyaltyProgram>
{
    public LoyaltyProgramChangingEvent(IEnumerable<GenericChangedEntry<LoyaltyProgram>> changedEntries)
        : base(changedEntries)
    {
    }
}
