using Stocki.Discord.Extensions;
using Stocki.Application.Extensions;
using Stocki.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDiscord();
builder.Services.AddConfiguration(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Logging.ClearProviders().AddConsole();

var app = builder.Build();

await app.RunAsync();
