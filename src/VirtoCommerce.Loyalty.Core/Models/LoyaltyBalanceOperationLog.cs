using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyBalanceOperationLog : AuditableEntity, ICloneable
{
    public string UserId { get; set; }

    public string OrganizationId { get; set; }

    /// <summary>
    /// "LoyaltyProgram" or "LoyaltyMission"
    /// </summary>
    public string SourceType { get; set; }

    /// <summary>
    /// Id of the program or mission
    /// </summary>
    public string SourceId { get; set; }

    public string ObjectId { get; set; }

    public string ObjectType { get; set; }

    /// <summary>
    /// Earned or redeemed
    /// </summary>
    public string OperationType { get; set; }

    public decimal Amount { get; set; }

    public decimal Balance { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
