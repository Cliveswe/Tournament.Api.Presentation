# Note: This is not a complete solution but a starting point for a tournament managing application.

# Tournament Management Solution

This repository provides a modular solution for managing tournaments and games, built with .NET 8. It includes a RESTful API, data access layer, and core business logic, making it suitable for sports leagues, e-sports, or any event requiring tournament scheduling and management.

## Projects

- **Tournament.Api**: ASP.NET Core Web API for managing tournaments and games.
- **Tournament.Data**: Data access layer using Entity Framework Core with SQL Server support.
- **Tournament.Core**: Core business logic and data transfer objects (DTOs).

## Features

- Create and manage tournaments with validated input.
- Schedule games and associate them with tournaments.
- Input validation using data annotations.
- RESTful endpoints with JSON and XML support.
- API documentation via Swagger (OpenAPI).
- Modular architecture with dependency injection and AutoMapper.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server instance (local or remote)

### Setup

1. Clone the repository.
2. Configure the connection string `TournamentApiContext` in `appsettings.json`.
3. Run database migrations (if applicable).
4. Start the API project:
5. Access Swagger UI at `https://localhost:<port>/swagger` for API exploration.

## Usage

- Use the API endpoints to create tournaments and schedule games.
- DTOs such as `TournamentDetailsCreateDto` and `GameCreateDto` ensure required fields and validation.
- Example payloads and endpoint documentation are available via Swagger UI.

## Dependencies

- ASP.NET Core 8
- Entity Framework Core 8 (with SQL Server)
- AutoMapper
- Swashbuckle (Swagger/OpenAPI)
- Newtonsoft.Json

## License

Distributed under the MIT License.