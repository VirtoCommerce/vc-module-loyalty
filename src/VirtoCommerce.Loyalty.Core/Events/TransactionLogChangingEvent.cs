using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class TransactionLogChangingEvent(IEnumerable<GenericChangedEntry<TransactionLog>> changedEntries)
    : GenericChangedEntryEvent<TransactionLog>(changedEntries)
{
}
