using System.Collections.Generic;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.Loyalty.Core.Events;

public class TransactionLogChangedEvent(IEnumerable<GenericChangedEntry<TransactionLog>> changedEntries)
    : GenericChangedEntryEvent<TransactionLog>(changedEntries);
