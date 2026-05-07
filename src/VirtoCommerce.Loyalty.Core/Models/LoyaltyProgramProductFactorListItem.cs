using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramProductFactorListItem : LoyaltyProgramProductFactor
{
    public CatalogProduct Product { get; set; }
}
