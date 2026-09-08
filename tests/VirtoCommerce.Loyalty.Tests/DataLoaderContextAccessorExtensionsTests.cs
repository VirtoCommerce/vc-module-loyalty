using System.Threading;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.ExperienceApi.Extensions;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.Loyalty.Tests;

[Trait("Category", "Unit")]
public class DataLoaderContextAccessorExtensionsTests
{
    [Fact]
    public async Task LoadLoyaltyObject_MissionProgressObjectType_ResolvesMissionObject()
    {
        var result = await LoadAsync(nameof(LoyaltyMissionProgress));

        Assert.NotNull(result);
        Assert.Equal("Mission", result.Type);
    }

    [Fact]
    public async Task LoadLoyaltyObject_ApplicationUserObjectType_ResolvesRegistrationObject()
    {
        var result = await LoadAsync(nameof(ApplicationUser));

        Assert.NotNull(result);
        Assert.Equal("Registration", result.Type);
    }

    [Fact]
    public async Task LoadLoyaltyObject_UnknownObjectType_ResolvesNull()
    {
        var result = await LoadAsync("SomeUnknownObjectType");

        Assert.Null(result);
    }

    private static async Task<LoyaltyOperationLogObject> LoadAsync(string objectType)
    {
        // The order data loader is only dispatched for the CustomerOrder branch, so no order service is needed here.
        var accessor = new DataLoaderContextAccessor { Context = new DataLoaderContext() };

        var loaderResult = accessor.LoadLoyaltyObject(
            customerOrderService: null,
            loaderKey: "loyalty_operation_log_object",
            objectId: "object-id",
            objectType: objectType);

        return await loaderResult.GetResultAsync(CancellationToken.None);
    }
}
