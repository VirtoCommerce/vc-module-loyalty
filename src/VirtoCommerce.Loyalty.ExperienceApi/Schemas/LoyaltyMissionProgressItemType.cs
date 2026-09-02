using GraphQL.DataLoader;
using GraphQL.Resolvers;
using GraphQL.Types;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.ExperienceApi.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Schemas;
using VirtoCommerce.XCatalog.Core.Models;
using VirtoCommerce.XCatalog.Core.Schemas;

namespace VirtoCommerce.Loyalty.ExperienceApi.Schemas;

public class LoyaltyMissionProgressItemType : ExtendableGraphType<LoyaltyMissionProgressItem>
{
    public LoyaltyMissionProgressItemType(IDataLoaderContextAccessor dataLoader)
    {
        Name = "LoyaltyMissionProgressItem";
        Description = "Per-SKU accumulation for a PerSku mission.";

        Field(x => x.ProductId, nullable: true).Description("The SKU product id.");
        Field(x => x.CurrentQuantity).Description("The accumulated quantity.");
        Field(x => x.TargetQuantity).Description("The target quantity.");

        var productField = new FieldType
        {
            Name = "product",
            Type = GraphTypeExtensionHelper.GetActualType<ProductType>(),
            Resolver = new FuncFieldResolver<LoyaltyMissionProgressItem, IDataLoaderResult<ExpProduct>>(context =>
                dataLoader.LoadProduct(
                    context, $"mission_progressItem_products_{context.Source.MissionProgressId}", context.Source.ProductId)),
        };
        AddField(productField);
    }
}
