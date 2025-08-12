## Package manager
```
Add-Migration Initial -Context VirtoCommerce.Loyalty.Data.Repositories.LoyaltyDbContext -Project VirtoCommerce.Loyalty.Data.SqlServer -StartupProject VirtoCommerce.Loyalty.Data.SqlServer -OutputDir Migrations -Verbose -Debug
```

### Entity Framework Core Commands
```
dotnet tool install --global dotnet-ef --version 8.*
```

**Generate Migrations**
```
dotnet ef migrations add Initial -- "{connection string}"
dotnet ef migrations add Update1 -- "{connection string}"
dotnet ef migrations add Update2 -- "{connection string}"
```
etc..

**Apply Migrations**
```
dotnet ef database update -- "{connection string}"
```
