// Ignore Spelling: Api Dto Ok

// -----------------------------------------------------------------------------
// File: GamesController.cs
// Summary: Handles HTTP API requests related to game entities within tournaments,
//          including retrieving, creating, updating, and deleting games.
//          Utilizes AutoMapper for DTO mapping and the Unit of Work pattern for
//          reliable and efficient data access management.
// <author> [Clive Leddy] </author>
// <created> [2025-06-28] </created>
// Notes: Implements RESTful endpoints with proper validation, status codes,
//        and comprehensive error handling to ensure API robustness and data integrity.
// -----------------------------------------------------------------------------


using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Dto;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;

namespace Tournament.Api.Controllers
{
    /// <summary>
    /// API controller for managing <see cref="Game"/> entities associated with tournaments.
    /// </summary>
    /// <remarks>
    /// Provides endpoints to retrieve, create, update, and delete games linked to specific tournament details.
    /// Utilizes AutoMapper for mapping between entities and DTOs, and implements the Unit of Work pattern for database operations.
    /// Validates game data including ensuring game times fall within the tournament period and preventing duplicates.
    /// Returns appropriate HTTP responses such as 200 OK, 201 Created, 204 No Content, 400 Bad Request, 404 Not Found, and 409 Conflict.
    /// </remarks>
    [ApiController]
    [Route("api/tournamentDetails/{tournamentId}/games")]
    //public class GamesController(TournamentApiContext context) : ControllerBase
    public class GamesController(IMapper mapper, IUoW uoW) : ControllerBase
    {
        #region GET api/Games api/1/Games/

        /// <summary>
        /// Retrieves all Game entities associated with a specific TournamentDetails ID.
        /// [HttpGet("/api/tournamentDetails/{tournametId}/games")]
        /// </summary>
        /// <param name="tournamentId">The ID of the tournament for which to fetch games.</param>
        /// <returns>
        /// Returns a 200 OK response containing a collection of GameDto objects if the tournament exists.
        /// Returns 404 Not Found if the tournament with the specified ID does not exist.
        /// </returns>
        /// Note that the route is overridden and it is important to add a root "/api/..." and 
        /// not "api/..." there is a difference.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetTournamentGame(int tournamentId)
        {
            // Validate the tournament ID from the route parameter
            bool exists = await uoW.TournamentDetailsRepository.AnyAsync(tournamentId);
            // If the tournament with the specified ID does not exist, return 404 Not Found
            if(!exists) {
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            // Fetch all Game records using the GameRepository
            //IEnumerable<Game> games = await uoW.GameRepository.GetAllAsync();

            // Check if any games exist for the specified tournament ID
            //IEnumerable<Game> gamesExist = await uoW.GameRepository.GetAllAsync();
            //List<Game> gamesResult = gamesExist.Where(g => g.TournamentDetailsId == tournamentId).ToList();

            IEnumerable<Game?> gamesResult = await uoW.GameRepository.GetByTournamentIdAsync(tournamentId);



            IEnumerable<GameDto> games = mapper.Map<IEnumerable<GameDto>>(gamesResult);
            // Alternatively, direct EF Core access could be used:
            //return await context.Game.ToListAsync();

            // Return the result with HTTP 200 OK
            return Ok(games);
        }

        /// <summary>
        /// Retrieves a single Game entity by its ID. GET: api/Games/5
        /// </summary>
        /// <param name="id">The ID of the gameEntity to retrieve.</param>
        /// <returns>
        /// Returns a 200 OK response with the gameEntity data if found;
        /// otherwise, returns 404 Not Found.
        /// </returns>
        [HttpGet("/api/games/{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            // Attempt to retrieve the gameEntity using the GameRepository
            Game? game = await uoW.GameRepository.GetAsync(id);

            // Return 404 if the gameEntity does not exist
            if(game == null) {
                return NotFound($"Game with ID {id} was not found.");
            }
            GameDto gameDto = mapper.Map<GameDto>(game);
            // Return the gameEntity with HTTP 200 OK
            // ASP.NET Core infers this as Ok(gameEntity) for ActionResult<T>
            return Ok(gameDto);// Returns 200 OK + JSON by default
        }

        #endregion

        #region PUT api/Games/5

        /// <summary>
        /// Updates an existing gameEntity identified by its ID with the provided data. PUT: api/Games/5
        /// </summary>
        /// <param name="id">The ID of the gameEntity to update.</param>
        /// <param name="gameUpdateDto">The updated gameEntity data provided in the request body.</param>
        /// <returns>
        /// Returns a <see cref="BadRequestResult"/> if the model state is invalid or the ID doesn't match.
        /// Returns <see cref="NotFoundResult"/> if the gameEntity does not exist.
        /// Returns <see cref="NoContentResult"/> if the update is successful.
        /// </returns>
        /// <remarks>
        /// This endpoint follows REST conventions for PUT operations. It uses AutoMapper to apply changes from the DTO to the entity.
        /// The GameUpdateDto includes only the fields allowed to be updated (e.g., Title).
        /// A 204 No Content response indicates that the update succeeded without returning a body.
        /// </remarks>
        [HttpPut("/api/games/{id}")]
        //public async Task<IActionResult> PutGame(int id, Game gameEntity)
        public async Task<IActionResult> PutGame(int id, [FromBody] GameUpdateDto gameUpdateDto)
        {
            if(!ModelState.IsValid) {
                // Return 400 Bad Request if the model state is invalid
                return BadRequest(ModelState);
            }

            Game? existingGame = await uoW.GameRepository.GetAsync(id);

            // Ensure the route ID matches the payload ID
            if(existingGame == null || existingGame.Id != id) {
                // 400 Bad Request if IDs don't match
                return BadRequest();
            }

            // Map the DTO to the existing entity.
            // This will update only the fields specified in the DTO.
            mapper.Map(gameUpdateDto, existingGame);

            try {
                // Attempt to save changes to the database
                uoW.GameRepository.Update(existingGame);

                // Persist the changes to the database
                await uoW.CompleteAsync();

            } catch(DbUpdateConcurrencyException) {

                bool exists = await GameExists(existingGame);
                // If the entity no longer exists, return 404 Not Found
                if(!exists) {
                    return NotFound($"Game with ID {existingGame.Id} was not found. It may have been deleted or does not exist.");
                } else {
                    // Otherwise re-throw the exception to bubble up
                    throw;
                }
            }


            // The update was successful; return HTTP 204 No Content as per REST convention
            return NoContent();
        }

        #endregion

        #region POST api/tournamentDetails/1/Games

        // POST: api/tournamentDetails/1/Games
        /// <summary>
        /// Creates a new game associated with the specified tournament.
        /// </summary>
        /// <param name="gameCreateDto">The data transfer object containing information about the game to create.</param>
        /// <param name="tournamentId">The ID of the tournament to which the game belongs (from route).</param>
        /// <returns>
        /// Returns:
        /// <list type="bullet">
        ///   <item><description><see cref="BadRequestObjectResult"/> if the input is invalid or IDs mismatch.</description></item>
        ///   <item><description><see cref="NotFoundResult"/> if the specified tournament does not exist.</description></item>
        ///   <item><description><see cref="ConflictResult"/> if a duplicate game exists with the same name and time.</description></item>
        ///   <item><description><see cref="ObjectResult"/> with HTTP 500 if saving to the database fails.</description></item>
        ///   <item><description><see cref="CreatedAtActionResult"/> with the created game if successful.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Validates the game data, checks for tournament existence and time constraints, 
        /// prevents duplicates, and saves the game to the database. 
        /// On success, returns the newly created game and its URI.
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<GameDto>> PostGame([FromBody] GameCreateDto gameCreateDto, [FromRoute] int tournamentId)
        {
            if(gameCreateDto == null) {
                // Return 400 Bad Request if the request body is null
                return BadRequest("Game data must be provided.");
            }

            // Validate the model state using data annotations.
            // ModelState contains validation errors if the DTO does not meet the required constraints.
            // This includes checks for required fields, string lengths, and range constraints.
            if(!ModelState.IsValid) {
                // Return a 400 Bad Request with the validation errors.
                return BadRequest(ModelState);
            }

            // This ensures that the game is being created for the correct tournament.
            // It is important to prevent mismatches between the route and the request body.
            // The route parameter is the tournamentId from the URL, while the DTO contains the tournamentDetailsId.
            // This check ensures that the game being created is associated with the correct tournament.
            if(gameCreateDto.TournamentDetailsId != tournamentId) {
                // Return 400 Bad Request if the tournament ID in the DTO does not match the route parameter.
                return BadRequest($"Mismatch between route tournament with Id {tournamentId} and request body.");
            }

            // Trim whitespace from the gameEntity name
            gameCreateDto.Name = gameCreateDto.Name.Trim();
            if(string.IsNullOrWhiteSpace(gameCreateDto.Name)) {
                // Return 400 Bad Request if the game name is empty or whitespace.
                return BadRequest("Game name is required.");
            }

            // Map the DTO to the TournamentDto
            TournamentDetails? existingTournament = await uoW.TournamentDetailsRepository.GetAsync(tournamentId);
            // Validate tournament existence
            if(existingTournament == null) {
                // Return 404 Not Found if the tournament does not exist
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            TournamentDto tournamentDetailsDto = mapper.Map<TournamentDto>(existingTournament);
            if(!IsGameTimeValid(gameCreateDto.Time, tournamentDetailsDto)) {
                // Return 400 Bad Request if the game time is not within the tournament's start and end dates.
                return BadRequest($"Game time must be within the tournament's start \"{tournamentDetailsDto.StartDate}\" and end \"{tournamentDetailsDto.EndDate}\" dates.");
            }

            // Optional: check if a similar gameEntity already exists.
            Game duplicateGame = await FindDuplicateGameAsync(gameCreateDto, tournamentId);

            if(duplicateGame != null) {
                // Return 409 Conflict if a duplicate gameEntity is found
                //return Conflict($"A gameEntity with the same name \"{gameCreateDto.Name}\" and date already exists.");
                return Conflict($"A game named \"{gameCreateDto.Name}\" already exists on {gameCreateDto.Time:yyyy-MM-dd HH:mm}.");
            }

            // This mapping will convert the DTO properties to the corresponding Game entity properties.
            // The Game entity will be used to persist the new game to the database.
            // This is necessary to ensure that the game is associated with the correct tournament.
            Game gameEntity = mapper.Map<Game>(gameCreateDto); // map DTO to Game entity

            try {
                // Add the new Game entity to the repository
                // This will prepare the entity to be inserted into the database.
                // The Unit of Work pattern is used to manage the database context and repositories.
                // This ensures that the game is added to the correct tournament.
                uoW.GameRepository.Add(gameEntity);

                // Persist the changes to the database
                var changes  = await uoW.CompleteAsync();
                if(changes == 0) {
                    // No changes saved - something went wrong
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save the new gameEntity.");
                }
            } catch(DbUpdateException) {
                // Log exception if you have logging setup
                return StatusCode(StatusCodes.Status500InternalServerError, "Database update error occurred.");
            }

            // Return 201 Created with the location header pointing to access the new resource.
            //nameof(GetGame): Specifies the action method (e.g., GetGame(int id)) that can be used to retrieve the created Game.
            //new { id = gameEntity.Id }: Supplies route values for the URL generation — here, it uses the newly created game’s ID.
            //mapper.Map<GameDto>(gameEntity): Returns the newly created game data in DTO form as the response body.
            return CreatedAtAction(nameof(GetGame), new { id = gameEntity.Id }, mapper.Map<GameDto>(gameEntity));
        }

        #endregion

        #region DELETE api/tournamentDetails/1/Games/5

        /// <summary>
        /// Deletes a game entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the game to delete.</param>
        /// <returns>
        /// Returns:
        /// - <see cref="BadRequestObjectResult"/> (400) if the ID is invalid.
        /// - <see cref="NotFoundObjectResult"/> (404) if no game with the specified ID exists.
        /// - <see cref="StatusCodeResult"/> (500) if a database error occurs during deletion.
        /// - <see cref="OkObjectResult"/> (200) with a confirmation message if the deletion succeeds.
        /// </returns>
        /// <remarks>
        /// This method attempts to remove a game from the database using the Unit of Work pattern.
        /// It validates the input ID, checks existence, and handles potential database exceptions.
        /// </remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame([FromRoute] int id)
        {
            // Validate the ID to ensure it is a positive integer.
            // If the ID is less than or equal to zero, return 400 Bad Request.
            if(id <= 0) {
                return BadRequest($"Invalid {id} specified for deletion.");
            }

            // Attempt to retrieve the Game entity by ID from the repository
            var game = await uoW.GameRepository.GetAsync(id);

            // If the entity does not exist, return 404 Not Found
            if(game == null) {
                return NotFound($"Game with the specified {id} was not found.");
            }

            try {
                // Remove the Game entity from the repository
                uoW.GameRepository.Remove(game);
                // Persist the change to the database
                await uoW.CompleteAsync();
            } catch(DbUpdateException) {
                // Return 500 Internal Server Error if a database update error occurs
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected concurrency error occurred.");
            }

            // Return 200 OK with a confirmation message
            return Ok(new { message = $"Game with ID {id} has bee deleted successfully." });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks whether a <see cref="Game"/> entity with the specified ID exists in the database.
        /// </summary>
        /// <param name="game">The <see cref="Game"/> entity whose ID is used to check for existence.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> representing the asynchronous operation, 
        /// with a result of <c>true</c> if a game with the given ID exists; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is typically used before performing update or delete operations to avoid conflicts 
        /// or exceptions when the entity no longer exists.
        /// </remarks>
        private async Task<bool> GameExists(Game game)
        {
            // Check for existence using the Game ID
            // Returns true if a Game with the same ID exists, otherwise false
            return await uoW.GameRepository.AnyAsync(game.Id);
        }

        /// <summary>
        /// Asynchronously checks for an existing game with the same name and date 
        /// within the specified tournament.
        /// </summary>
        /// <param name="gameCreateDto">The game data containing the name and time of the game to check for duplicates.</param>
        /// <param name="tournamentId">The ID of the tournament to search within.</param>
        /// <returns>
        /// A <see cref="Game"/> instance if a duplicate is found; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method delegates to the repository to determine if a game with the same name and 
        /// scheduled time already exists within the given tournament.
        /// </remarks>
        private async Task<Game> FindDuplicateGameAsync(GameCreateDto gameCreateDto, int tournamentId)
        {

            // Check if a gameEntity with the same name and time already exists in the specified tournament
            return await uoW.GameRepository.GetByNameAndDateAsync(gameCreateDto.Name, tournamentId);
        }

        /// <summary>
        /// Validates whether the specified game time falls within the start and end dates of the tournament.
        /// </summary>
        /// <param name="gameTime">The time of the game to validate.</param>
        /// <param name="tournament">The tournament against which the game time is validated.</param>
        /// <returns>
        /// <c>true</c> if the game time is strictly between the tournament's start and end dates; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method ensures that the game is scheduled within the tournament's valid timeframe.
        /// </remarks>
        private bool IsGameTimeValid(DateTime gameTime, TournamentDto tournament)
        {
            return gameTime > tournament.StartDate && gameTime < tournament.EndDate;
        }

        #endregion
    }
}
