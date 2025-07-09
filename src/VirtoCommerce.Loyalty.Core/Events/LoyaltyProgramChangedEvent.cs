using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class LoyaltyProgramChangedEvent : GenericChangedEntryEvent<LoyaltyProgram>
{
    public LoyaltyProgramChangedEvent(IEnumerable<GenericChangedEntry<LoyaltyProgram>> changedEntries)
        : base(changedEntries)
    {
    }
}
