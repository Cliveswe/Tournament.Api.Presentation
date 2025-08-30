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

## API Endpoints

### Games Controller (`api/tournamentDetails/{tournamentId}/games`)

#### GET Endpoints
- `GET api/tournamentDetails/{tournamentId}/games` - Get all games for a tournament with pagination
- `GET api/tournamentDetails/{tournamentId}/games/{id}` - Get a specific game by ID
- `GET api/tournamentDetails/{tournamentId}/games/title/{title}` - Get a game by title

#### POST Endpoint
- `POST api/tournamentDetails/{tournamentId}/games` - Create a new game in a tournament
  - Accepts: `application/json`
  - Returns: 201 Created with game details

#### PUT Endpoint
- `PUT api/tournamentDetails/{tournamentId}/games?title={title}` - Update a game by title
  - Accepts: `application/json`
  - Returns: 200 OK with updated game

#### PATCH Endpoint
- `PATCH api/tournamentDetails/{tournamentId}/games/{id}` - Partially update a game
  - Accepts: `application/json-patch+json`
  - Returns: 200 OK with updated game

#### DELETE Endpoint
- `DELETE api/tournamentDetails/{tournamentId}/games/{id}` - Delete a game from a tournament

  ## API Endpoints

### Tournament Details Controller (`api/tournamentDetails`)

#### GET Endpoints
- `GET api/tournamentDetails`  
  Retrieve all tournaments.

- `GET api/tournamentDetails/{id}`  
  Retrieve a specific tournament by ID.

#### POST Endpoint
- `POST api/tournamentDetails`  
  Create a new tournament.  
  Accepts: `application/json`  
  Returns: 201 Created with tournament details.

#### PUT Endpoint
- `PUT api/tournamentDetails/{id}`  
  Update an existing tournament by ID.  
  Accepts: `application/json`  
  Returns: 200 OK with updated tournament.

#### PATCH Endpoint
- `PATCH api/tournamentDetails/{id}`  
  Partially update a tournament using JSON Patch.  
  Accepts: `application/json-patch+json`  
  Returns: 200 OK with updated tournament.

#### DELETE Endpoint
- `DELETE api/tournamentDetails/{id}`  
  Delete a tournament by ID.  
  Returns: 200 OK with confirmation message.

---

Add this section under your existing API Endpoints in the README.md.  
This documents the main CRUD operations for tournaments, matching your controller’s conventions and .NET 8 best practices.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server instance (local or remote)

### Setup

1. Clone the repository.
2. Configure the connection string `TournamentApiContext` in `appsettings.json`.
3. Run database migrations (if applicable).
4. Start the API project.
5. Access Swagger UI at `https://localhost:<port>/swagger` for API exploration.

## Usage

- Use the API endpoints to create tournaments and schedule games.
- DTOs such as `TournamentDetailsCreateDto` and `GameCreateDto` ensure required fields and validation.
- Example payloads and endpoint documentation are available via Swagger UI.
- Supports JSON Patch for partial updates to game resources.

## Dependencies

- ASP.NET Core 8
- Entity Framework Core 8 (with SQL Server)
- AutoMapper
- Swashbuckle (Swagger/OpenAPI)
- Newtonsoft.Json

## License

Distributed under the MIT License.  
