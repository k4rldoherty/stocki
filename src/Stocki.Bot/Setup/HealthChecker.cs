namespace Stocki.Bot.Setup;

public class HealthChecker
{
    public static void SetupHealthEndpoint(IWebHostBuilder webBuilder)
    {
        webBuilder.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(8080);
        });
        webBuilder.Configure(app =>
        {
            // Add a simple health check endpoint at the root path "/"
            app.UseRouting(); // Required for MapGet
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMethods(
                    "/",
                    new[] { "GET", "HEAD" },
                    async context =>
                    {
                        if (context.Request.Method == "HEAD")
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                        }
                        else
                        {
                            await context.Response.WriteAsync("Bot is alive!"); // Respond with a simple message
                        }
                    }
                );
            });
        });
    }
}
