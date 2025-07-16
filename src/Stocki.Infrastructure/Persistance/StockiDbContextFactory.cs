using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Stocki.Infrastructure.Persistance;

public class StockiDbContextFactory : IDesignTimeDbContextFactory<StockiDbContext>
{
    public StockiDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var connectionString = config.GetSection("Postgres").GetSection("ConnectionString").Value;
        var opt = new DbContextOptionsBuilder<StockiDbContext>();
        opt.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly(typeof(StockiDbContext).Assembly.FullName)
        );
        return new StockiDbContext(opt.Options);
    }
}
