using System;
using GraphQL;
using GraphQL.MicrosoftDI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Handlers;
using VirtoCommerce.Loyalty.Data.MySql;
using VirtoCommerce.Loyalty.Data.PostgreSql;
using VirtoCommerce.Loyalty.Data.Provider;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.Loyalty.Data.Services;
using VirtoCommerce.Loyalty.Data.SqlServer;
using VirtoCommerce.Loyalty.ExperienceApi;
using VirtoCommerce.Loyalty.ExperienceApi.Authorization;
using VirtoCommerce.Loyalty.ExperienceApi.TypeHooks;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.MySql.Extensions;
using VirtoCommerce.Platform.Data.PostgreSql.Extensions;
using VirtoCommerce.Platform.Data.SqlServer.Extensions;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Loyalty.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        _ = new GraphQLBuilder(serviceCollection, builder =>
        {
            builder.AddSchema(serviceCollection, typeof(XapiAssemblyMarker));
            builder.AddGraphTypeHook<ProductTypeHook>();
        });
        serviceCollection.AddSingleton<ScopedSchemaFactory<XapiAssemblyMarker>>();

        serviceCollection.AddDbContext<LoyaltyDbContext>(options =>
        {
            var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
            var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

            switch (databaseProvider)
            {
                case "MySql":
                    options.UseMySqlDatabase(connectionString, typeof(MySqlDataAssemblyMarker), Configuration);
                    break;
                case "PostgreSql":
                    options.UsePostgreSqlDatabase(connectionString, typeof(PostgreSqlDataAssemblyMarker), Configuration);
                    break;
                default:
                    options.UseSqlServerDatabase(connectionString, typeof(SqlServerDataAssemblyMarker), Configuration);
                    break;
            }
        });

        // Register services
        serviceCollection.AddTransient<ILoyaltyRepository, LoyaltyRepository>();
        serviceCollection.AddSingleton<Func<ILoyaltyRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<ILoyaltyRepository>());

        serviceCollection.AddTransient<ILoyaltyProgramService, LoyaltyProgramService>();
        serviceCollection.AddTransient<ILoyaltyProgramSearchService, LoyaltyProgramSearchService>();

        serviceCollection.AddTransient<ILoyaltyProgramOperationLogService, LoyaltyProgramOperationLogService>();
        serviceCollection.AddTransient<ILoyaltyProgramOperationLogSearchService, LoyaltyProgramOperationLogSearchService>();

        serviceCollection.AddTransient<ILoyaltyProgramProductFactorService, LoyaltyProgramProductFactorService>();
        serviceCollection.AddTransient<ILoyaltyProgramProductFactorSearchService, LoyaltyProgramProductFactorSearchService>();

        serviceCollection.AddTransient<ILoyaltyLogicService, LoyaltyLogicService>();
        serviceCollection.AddTransient<IProductLoyaltyProgramService, LoyaltyLogicService>();

        serviceCollection.AddTransient<LoyaltyProgramHandler>();

        serviceCollection.AddTransient<LoyaltyPaymentMethod>();

        serviceCollection.AddSingleton<IAuthorizationHandler, CanAccessLoyaltyAuthorizationHandler>();
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        appBuilder.UseScopedSchema<XapiAssemblyMarker>("loyalty");

        var serviceProvider = appBuilder.ApplicationServices;

        // Register settings
        var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        // Register store settings
        settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.StoreSettings, nameof(Store));

        // Register permissions
        var permissionsRegistrar = serviceProvider.GetRequiredService<IPermissionsRegistrar>();
        permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "Loyalty", ModuleConstants.Security.Permissions.AllPermissions);

        // Apply migrations
        using var serviceScope = serviceProvider.CreateScope();
        using var dbContext = serviceScope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();
        dbContext.Database.Migrate();

        foreach (var conditionTree in AbstractTypeFactory<LoyaltyProgramConditionAndRewardTreePrototype>.TryCreateInstance().Traverse<IConditionTree>(x => x.AvailableChildren))
        {
            AbstractTypeFactory<IConditionTree>.RegisterType(conditionTree.GetType());
        }

        foreach (var conditionTree in AbstractTypeFactory<LoyaltyProgramProductConditionTreePrototype>.TryCreateInstance().Traverse<IConditionTree>(x => x.AvailableChildren))
        {
            AbstractTypeFactory<IConditionTree>.RegisterType(conditionTree.GetType());
        }

        appBuilder.RegisterEventHandler<OrderChangedEvent, LoyaltyProgramHandler>();
        appBuilder.RegisterEventHandler<UserChangedEvent, LoyaltyProgramHandler>();

        // Register payment method
        var paymentMethodsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPaymentMethodsRegistrar>();
        paymentMethodsRegistrar.RegisterPaymentMethod(() =>
            appBuilder.ApplicationServices.GetService<LoyaltyPaymentMethod>());
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
