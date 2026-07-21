using System;

namespace VirtoCommerce.Loyalty.Core.Models;

/// <summary>
/// Per-SKU accumulation for a PerSku mission progress.
/// </summary>
public class LoyaltyMissionProgressItem : ICloneable
{
    public string MissionId { get; set; }

    public string MissionProgressId { get; set; }

    public string ProductId { get; set; }

    public int CurrentQuantity { get; set; }

    public int TargetQuantity { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }

}
