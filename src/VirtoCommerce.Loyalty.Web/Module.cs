using GraphQL.MicrosoftDI;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.MySql.Extensions;
using VirtoCommerce.Platform.Data.PostgreSql.Extensions;
using VirtoCommerce.Platform.Data.SqlServer.Extensions;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Data.MySql;
using VirtoCommerce.Loyalty.Data.PostgreSql;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.Loyalty.Data.SqlServer;
using VirtoCommerce.Loyalty.ExperienceApi;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Services;
using System;

namespace VirtoCommerce.Loyalty.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
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

        // Register GraphQL schema
        _ = new GraphQLBuilder(serviceCollection, builder =>
        {
            builder.AddSchema(serviceCollection, typeof(XapiAssemblyMarker));
        });

        serviceCollection.AddSingleton<ScopedSchemaFactory<XapiAssemblyMarker>>();

        serviceCollection.AddTransient<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
        serviceCollection.AddTransient<Func<ILoyaltyProgramRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<ILoyaltyProgramRepository>());
        serviceCollection.AddTransient<ILoyaltyProgramService, LoyaltyProgramService>();
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var serviceProvider = appBuilder.ApplicationServices;

        // Register settings
        var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        // Register permissions
        var permissionsRegistrar = serviceProvider.GetRequiredService<IPermissionsRegistrar>();
        permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "Loyalty", ModuleConstants.Security.Permissions.AllPermissions);

        // Register partial GraphQL schema
        appBuilder.UseScopedSchema<XapiAssemblyMarker>("loyalty");

        // Apply migrations
        using var serviceScope = serviceProvider.CreateScope();
        using var dbContext = serviceScope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();
        dbContext.Database.Migrate();
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
