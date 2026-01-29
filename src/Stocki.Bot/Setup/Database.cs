using Microsoft.EntityFrameworkCore;
using Stocki.Infrastructure.Persistance;
namespace Stocki.Bot.Setup;

public class Database
{
    public static void SetupDatabase(HostBuilderContext context, IServiceCollection services)
    {
        var connectionString = context
            .Configuration.GetSection("Postgres")
            .GetSection("ConnectionString")
            .Value;
        services.AddDbContext<StockiDbContext>(opt =>
        {
            opt.UseNpgsql(connectionString);
        });
    }
}
