# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

COPY ["src/Stocki.Bot/Stocki.Bot.csproj", "src/Stocki.Bot/"]
COPY ["src/Stocki.Application/Stocki.Application.csproj", "src/Stocki.Application/"]
COPY ["src/Stocki.Domain/Stocki.Domain.csproj", "src/Stocki.Domain/"]
COPY ["src/Stocki.Infrastructure/Stocki.Infrastructure.csproj", "src/Stocki.Infrastructure/"]
COPY ["src/Stocki.PriceMonitoringService/Stocki.PriceMonitoringService.csproj", "src/Stocki.PriceMonitoringService/"]
COPY ["src/Stocki.NotificationService/Stocki.NotificationService.csproj", "src/Stocki.NotificationService/"]
COPY ["src/Stocki.Shared/Stocki.Shared.csproj", "src/Stocki.Shared/"]

RUN dotnet restore "src/Stocki.Bot/Stocki.Bot.csproj"

COPY . .

WORKDIR /app/src/Stocki.Bot
RUN dotnet publish "Stocki.Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build-env /app/publish .

COPY ["src/Stocki.Bot/appsettings.json", "src/Stocki.Bot/appsettings.json"]

ENTRYPOINT ["dotnet", "Stocki.Bot.dll"]
