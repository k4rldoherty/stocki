using Microsoft.EntityFrameworkCore;
using Stocki.Infrastructure.Persistance;
namespace Stocki.Bot.Setup;

public class Database
{
    public static void SetupDatabase(HostBuilderContext context, IServiceCollection services)
    {
        var envType = context.HostingEnvironment.EnvironmentName;
        Console.WriteLine(envType);
        var section = envType == "Production" ? "production" : "local";
        var connectionString = context
              .Configuration.GetSection("ConnectionStrings")
              .GetSection(section)
              .Value;

        if (string.IsNullOrEmpty(connectionString)) throw new NullReferenceException("Connection string is null or empty.");

        services.AddDbContext<StockiDbContext>(opt =>
        {
            opt.UseNpgsql(connectionString);
        });
    }
}
