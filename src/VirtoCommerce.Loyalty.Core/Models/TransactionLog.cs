using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class TransactionLog : AuditableEntity, ICloneable
{
    public string LoyaltyProgramId { get; set; }

    public string CustomerId { get; set; }

    public LoyaltyOperationType OperationType { get; set; }

    public decimal AccruedPoints { get; set; }

    public DateTime Date { get; set; }

    public string ObjectId { get; set; }

    public string ObjectType { get; set; }

    public string Comment { get; set; }

    public decimal Balance { get; set; }

    public object Clone() => (TransactionLog)MemberwiseClone();
}
