# Stocki - Your Discord Stock Companion
<img width="1240" height="1072" alt="Stocki-News" src="https://github.com/user-attachments/assets/8c345454-fb25-43c3-92bd-7b6c15529a6c" />
<img width="1240 height="1072" alt="Stocki Quote" src="https://github.com/user-attachments/assets/2a6e11b1-2b07-4aab-9038-f511281b98ce" />


Stocki is a Discord bot designed to provide quick and comprehensive financial information right within your Discord server. Leveraging modern .NET technologies and a clean architectural approach, Stocki aims to deliver reliable stock data and a smooth user experience.

## Features

Stocki currently supports the following slash commands:

* `/overview [ticker]`: Provides an in-depth financial overview of a specified stock ticker (e.g., `AAPL`, `MSFT`). This includes details like company description, sector, industry, market capitalization, P/E ratio, dividend yield, and 52-week high/low.
* `/quote [ticker]`: Fetches real-time (or near real-time) quote information for a given stock ticker. This includes current price, open, close, high, low, and percentage change.
* `/get-company-news [ticker]`: Gets the top x news articles for a given stock, containing a title, summary and link to the article.

## Technologies Used

* **.NET 9 (C#):** The core framework for the bot's development.
* **Discord.Net:** A powerful and flexible library for interacting with the Discord API.
* **MediatR:** Implemented to facilitate a clear separation of concerns using the Mediator pattern for handling commands and queries.
* **Microsoft.Extensions.Hosting:** For managing the bot's lifecycle as a hosted service.
* **Microsoft.Extensions.Logging:** For structured and configurable logging.
* **Microsoft.Extensions.Configuration:** For robust application configuration.
* **HttpClient:** For making external API calls to financial data providers.
* **Newtonsoft.Json:** For JSON serialization and deserialization.
* **AlphaVantage API:** Used as a primary data source for stock overviews.
* **Finnhub API:** Used as a primary data source for stock quotes.
* **Memory Cache:** For efficient caching of API responses to reduce redundant calls.
* **xUnit:** Unit testing framework
* **Faker / Bogus:** Generating mock data for unit tests

## Architecture

Stocki is built following **Clean Architecture** principles, enhanced by the **Mediator pattern** (using MediatR). This design ensures a highly maintainable, testable, and scalable application structure:

* **Presentation Layer (`Stocki.Bot`):** Handles Discord interactions, parses commands, orchestrates requests via `IMediator`, and formats responses. It knows nothing about the business logic or data fetching.
* **Application Layer (`Stocki.Application`):** Contains the application's use cases.
    * **Queries/Commands:** Simple data structures defining user intents (e.g., `StockOverviewQuery`).
    * **Handlers:** Classes containing the business logic for processing a specific query or command (e.g., `StockOverviewQueryHandler`). They coordinate between domain and infrastructure layers.
    * **Interfaces:** Defines contracts for external services (e.g., `IAlphaVantageClient`).
* **Domain Layer (`Stocki.Domain`):** Holds the core business rules and entities.
    * **Models:** Core business entities (e.g., `StockOverview`, `StockQuote`).
    * **Value Objects:** Immutable objects that encapsulate specific domain concepts and their validation rules (e.g., `TickerSymbol`).
* **Infrastructure Layer (`Stocki.Infrastructure`):** Implements the interfaces defined in the Application layer, handling external concerns like API communication (e.g., `AlphaVantageClient`, `FinnhubClient`), data mapping, and caching.

This layered approach promotes:
* **Decoupling:** Components are independent and can be changed without affecting others.
* **Testability:** Each layer can be easily tested in isolation.
* **Maintainability:** Clear responsibilities make the codebase easier to understand and evolve.

## Contributing

To run Stocki locally, you'll need to obtain API keys for the financial data providers.

### Prerequisites

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or newer installed.

### Quick Guide
* Clone the repo
* Run ```dotnet restore```
* Run ```dotnet test```
* Create a new feature branch
* When you add a new feature - write some unit tests and do some manual tests
* Create a pr!

### 1. API Key Setup

Stocki uses external APIs for financial data. You'll need API keys from:

* **AlphaVantage:**
    1.  Register at [AlphaVantage](https://www.alphavantage.co/).
    2.  Obtain your API key from your account dashboard.
* **Finnhub:**
    1.  Register at [Finnhub](https://finnhub.io/).
    2.  Obtain your API key from your dashboard.

### 2. Configuration

Create an `appsettings.json` file in the root of the `src/Stocki.Bot` project (next to `Program.cs`) and populate it with your credentials:

Make sure this file is part of your .gitignore!

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AlphaVantage": {
    "BaseUrl": "[https://www.alphavantage.co/](https://www.alphavantage.co/)",
    "ApiKey": "YOUR_ALPHA_VANTAGE_API_KEY"
  },
  "Finnhub": {
    "BaseUrl": "[https://finnhub.io/api/v1/](https://finnhub.io/api/v1/)",
    "ApiKey": "YOUR_FINNHUB_API_KEY"
  },
  "Discord": {
    "Token": "YOUR_DISCORD_BOT_TOKEN"
  }
}
