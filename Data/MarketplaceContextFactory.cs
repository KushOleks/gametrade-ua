using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace OrderSystem.Data;
// Migrations can inspect the model without starting the web server or seeding data.
public class MarketplaceContextFactory : IDesignTimeDbContextFactory<MarketplaceContext>
{
    public MarketplaceContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables().Build();
        Directory.CreateDirectory("App_Data");
        var connection = configuration.GetConnectionString("Marketplace") ?? "Data Source=App_Data/marketplace.db";
        return new(new DbContextOptionsBuilder<MarketplaceContext>().UseSqlite(connection).Options);
    }
}
