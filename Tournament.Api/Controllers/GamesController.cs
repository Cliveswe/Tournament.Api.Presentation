// Ignore Spelling: Api Dto Ok

// --------------------------------------------------------------------------------
// File: GamesController.cs
// Summary: Handles HTTP API requests related to game entities within tournaments,
//          including retrieving, creating, updating, patching, and deleting games.
//          Utilizes AutoMapper for DTO mapping and the Unit of Work pattern for
//          reliable and efficient data access management.
// <author> [Clive Leddy] </author>
// <created> [2025-06-28] </created>
// Notes: Implements RESTful endpoints with proper validation, concurrency control,
//        appropriate status codes (200, 201, 204, 400, 404, 409, 500), and
//        comprehensive error handling to ensure API robustness and data integrity.
// --------------------------------------------------------------------------------
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Tournament.Core.Dto;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;

namespace Tournament.Api.Controllers
{
    /// <summary>
    /// API controller for managing <see cref="Game"/> entities associated with tournaments.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints to perform CRUD operations on games within specific tournaments:
    /// <list type="number">
    /// <item>Retrieves games by tournamentEntity ID or by game ID.</item>
    /// <item>Creates new games linked to tournaments, ensuring data validity and preventing duplicates.</item>
    /// <item>Updates or partially updates existing games with full validation and concurrency handling.</item>
    /// <item>Deletes games by their ID with appropriate validation and error handling.</item>
    /// <item>Utilizes AutoMapper to map between entity models and data transfer objects (DTOs).</item>
    /// <item>Implements the Unit of Work pattern to coordinate database operations through repositories.</item>
    /// <item>Validates that game times fall within the tournamentEntity’s start and end dates.</item>
    /// <item>Returns standard HTTP response codes such as 200 OK, 201 Created, 204 No Content, 
    /// 400 Bad Request, 404 Not Found, 409 Conflict, and 500 Internal Server Error.</item>
    /// </list>
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
        /// <param name="tournamentId">The ID of the tournamentEntity for which to fetch games.</param>
        /// <returns>
        /// Returns a 200 OK response containing a collection of GameDto objects if the tournamentEntity exists.
        /// Returns 400 Bad Request, if the tournamentEntity ID is invalid. 
        /// Returns 404 Not Found if the tournamentEntity with the specified ID does not exist.
        /// </returns>
        /// Note that the route is overridden and it is important to add a root "/api/..." and 
        /// not "api/..." there is a difference.
        // GET api/tournamentDetails/{tournamentId}/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetTournamentGame(int tournamentId)
        {

            #region Validation of input parameters
            // Validate the tournamentEntity ID from the route parameter
            if(tournamentId <= 0) {
                // If the tournamentEntity ID is invalid (less than or equal to zero), return 400 Bad Request.
                // This ensures that the client must provide a valid tournamentEntity ID to retrieve games.
                return BadRequest($"Invalid tournamentEntity ID {tournamentId}.");
            }
            #endregion

            // Validate the tournamentEntity ID from the route parameter
            bool exists = await uoW.TournamentDetailsRepository.AnyAsync(tournamentId);
            // If the tournamentEntity with the specified ID does not exist, return 404 Not Found
            if(!exists) {
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            // Retrieve all games associated with the specified tournamentEntity ID.
            IEnumerable<Game?> gamesResult = await uoW.GameRepository.GetByTournamentIdAsync(tournamentId);

            // Map the Game entities to GameDto using AutoMapper.
            IEnumerable<GameDto> games = mapper.Map<IEnumerable<GameDto>>(gamesResult);

            // Return the result with HTTP 200 OK
            return Ok(games);
        }


        /// <summary>
        /// Retrieves a specific game by its unique identifier, ensuring it belongs to the specified tournamentEntity.
        /// </summary>
        /// <param name="tournamentId">The ID of the tournamentEntity the game is associated with.</param>
        /// <param name="id">The unique ID of the game to retrieve.</param>
        /// <returns>
        /// Returns a 200 OK response with the corresponding <see cref="GameDto"/> if found and associated with the tournamentEntity.
        /// Returns 400 Bad Request if either the game ID or tournamentEntity ID is invalid.
        /// Returns 404 Not Found if the game does not exist or does not belong to the specified tournamentEntity.
        /// </returns>
        /// <remarks>
        /// This method performs the following validations:
        /// <list type="number">
        /// <item>Ensures both <paramref name="tournamentId"/> and <paramref name="id"/> are positive integers.</item>
        /// <item>Fetches the game from the repository by ID.</item>
        /// <item>Verifies that the game's tournamentEntity ID matches the provided tournamentEntity ID.</item>
        /// <item>Uses AutoMapper to convert the entity to a DTO and returns the result.</item>
        /// </list>
        /// </remarks>
        // GET api/tournamentDetails/{tournamentId}/games/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GameDto>> GetGameById(int tournamentId, int id)
        {
            #region Validation of Input parameters
            // Validate the tournamentEntity ID and game ID
            if(id <= 0 || tournamentId <= 0) {
                return BadRequest("Invalid tournamentEntity id or game id.");
            }
            #endregion

            #region Validation of Tournament existence
            // Check if the tournamentEntity exists
            var gameEntity = await uoW.GameRepository.GetByIdAsync(id);

            // If the game does not exist, return 404 Not Found
            if(gameEntity == null) {
                return NotFound($"Game with ID {id} was not found.");
            }

            // Ensure the game belongs to the specified tournamentEntity.
            // This check ensures that the game being retrieved is associated with the correct tournamentEntity.
            if(gameEntity.TournamentDetailsId != tournamentId) {
                return NotFound($"Game with ID {id} does not belong to the specified tournamentEntity.");
            }
            #endregion 

            // Map the Game entity to a GameDto using AutoMapper
            var gameDto = mapper.Map<GameDto>(gameEntity);

            // Return the GameDto with HTTP 200 OK
            return Ok(gameDto);
        }

        /// <summary>
        /// Retrieves a game by its title within a specific tournamentEntity.
        /// </summary>
        /// <param name="tournamentId">The ID of the tournamentEntity to search within.</param>
        /// <param name="title">The title of the game to retrieve. Cannot be null or empty.</param>
        /// <returns>
        /// An <see cref="ActionResult{GameDto}"/> containing the game data if found, or an appropriate HTTP error response:
        /// - 400 Bad Request if inputs are invalid
        /// - 404 Not Found if no matching game is found
        /// </returns>
        /// <remarks>
        /// This method trims the input title and filters by both game title and tournamentEntity ID.
        /// It ensures games from other tournaments with the same title are excluded.
        /// </remarks>
        // GET api/tournamentDetails/{tournamentId}/games/byTitle/{title}
        [HttpGet("byTitle/{title}")]
        public async Task<ActionResult<GameDto>> GetGameByTitle(int tournamentId, string title)
        {
            #region VAlidation of input parameters
            // Validate the title input.
            if(string.IsNullOrWhiteSpace(title)) {
                return BadRequest("Title must be a non-empty string.");
            }

            // Trim whitespace from the title to ensure accurate matching.
            string trimmedTitle = title.Trim();

            // Validate the tournamentEntity ID.
            if(tournamentId <= 0) {
                return BadRequest("Invalid tournamentEntity id.");
            }
            #endregion

            // Check if the tournamentEntity exists.
            // This check ensures that the game being retrieved is associated with the correct tournamentEntity.
            Game? gameEntity = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(trimmedTitle, tournamentId);

            // If no game with the specified title exists in the tournamentEntity, return 404 Not Found.
            // This ensures that the API does not return a game from a different tournamentEntity with the same title.
            if(gameEntity == null) {
                return NotFound($"Game with title '{trimmedTitle}' was not found.");
            }

            // Map the Game entity to a GameDto using AutoMapper.
            GameDto gameDto = mapper.Map<GameDto>(gameEntity);

            // Return the GameDto with HTTP 200 OK.
            return Ok(gameDto);
        }


        #endregion

        #region PUT api/tournamentDetails/1/Games/5
        [HttpPut]
        public async Task<IActionResult> PutGame(int tournamentId, [FromQuery] string title, [FromBody] GameUpdateDto gameUpdateDto)
        {
            #region Validation of Input parameters
            // Validate the input parameters.
            if(!ModelState.IsValid) {
                // Return 400 Bad Request if the model state is invalid
                return BadRequest(ModelState);
            }

            // Validate the tournamentEntity ID from the route parameter.
            if(tournamentId <= 0) {
                // Return 400 Bad Request if the tournamentEntity ID is invalid
                return BadRequest($"Invalid tournamentEntity ID {tournamentId}.");
            }

            // Validate the game title input
            title = title.Trim();
            if(string.IsNullOrWhiteSpace(title)) {
                return BadRequest("Game title must be a non-empty string.");
            }
            #endregion

            #region Validation of Game entity
            //Get a game entity by title and tournament id.
            Game? gameEntity = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(title, tournamentId);

            //Validate that the game exists.
            if(gameEntity == null) {
                // 400 Bad Request if IDs don't match
                return NotFound($"Game with title '{title}' was not found.");
            }
            #endregion

            // Map the DTO to the existing entity.
            // This will update only the fields specified in the DTO.
            mapper.Map(gameUpdateDto, gameEntity);

            // Attempt to save changes to the database
            uoW.GameRepository.Update(gameEntity);

            // Persist the changes to the database.
            var result = await uoW.CompleteAsync();
            // If no changes were saved, return 500 Internal Server Error.
            if(result == 0) {
                // If no changes were saved, return 500 Internal Server Error
                return StatusCode(500, "Update failed. No changes were saved.");
            }

            // The update was successful; return HTTP 204 No Content as per REST convention.
            return NoContent();
        }

        #endregion

        #region PATCH api/tournamentDetails/1/Games/5

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchGame(int tournamentId, int id, [FromBody] JsonPatchDocument<GameDto> patchDocument)
        {
            #region Validation of Input Parameters

            // Ensure the incoming patch document is not null.
            if(patchDocument == null) {
                // If the model state is invalid, return 400 Bad Request with validation errors.
                return BadRequest("Patch document cannot be null.");
            }

            // Check if the tournamentEntity ID is valid.
            if(tournamentId <= 0) {
                // If the tournamentEntity ID is invalid, return 400 Bad Request with an error message.
                return BadRequest($"Invalid tournamentEntity ID {tournamentId} specified for patching.");
            }

            // Check if the game ID is valid.
            if(id <= 0) {
                // If the game ID is invalid, return 400 Bad Request with an error message.
                return BadRequest($"Invalid game ID {id} specified for patching.");
            }
            #endregion

            #region Validation of Model State before applying the patch

            // Fetch the existing game by ID.
            Game? gameEntity = await uoW.GameRepository.GetByIdAsync(id);

            // If the game does not exist or the tournamentEntity ID does not match, return 404 Not Found.
            if(gameEntity == null || gameEntity.TournamentDetailsId != tournamentId) {
                return NotFound($"Game with ID {id} in Tournament {tournamentId} was not found.");
            }

            // Fetch the tournamentEntity to validate StartDate.
            TournamentDetails? tournamentEntity = await uoW.TournamentDetailsRepository.GetAsync(tournamentId);
            if(tournamentEntity == null) {
                // If the tournamentEntity does not exist, return 404 Not Found.
                // Note: This is a different check than the one above, as it checks the tournamentEntity existence.
                return NotFound($"Tournament with ID {tournamentId} does not exist and the game will not be updated.");
            }
            #endregion

            #region Validate Game date is within the Tournament period

            // Map tournamentEntity to TournamentDto.
            var tournamentDto = mapper.Map<TournamentDto>(tournamentEntity);

            // Map gameEntity to a GameDto.
            GameDto gameToPatch = mapper.Map<GameDto>(gameEntity);

            // Validation of Model State after Patch Application.
            // Apply patch to DTO and validate model state.
            patchDocument.ApplyTo(gameToPatch, ModelState);

            // Validate the model state after applying the patch.
            if(!TryValidateModel(gameToPatch)) {
                // If the model state is invalid after applying the patch, return 400 Bad Request with validation errors.
                return ValidationProblem(ModelState);
            }

            // Validate StartDate is within tournamentEntity period
            if(!IsGameTimeValid(gameToPatch.StartDate, tournamentDto)) {
                return BadRequest($"StartDate must be within the tournamentEntity's start \"{tournamentDto.StartDate}\" and end \"{tournamentDto.EndDate}\" dates.");
            }

            #endregion

            // Map patched DTO back to the game
            //gameEntity.Title = gameToPatch.Title;
            //gameEntity.Time = gameToPatch.StartDate;
            mapper.Map(gameToPatch, gameEntity);

            // Attempt to update the game in the repository
            // Update the existing gameEntity in the repository
            uoW.GameRepository.Update(gameEntity);
            // Persist the changes to the database
            var result = await uoW.CompleteAsync();

            // If no changes were saved, return 500 Internal Server Error
            if(result == 0) {
                return StatusCode(500, "Update failed. No changes were saved.");
            }

            // Return 200 OK with the updated GameDto.
            // This indicates that the patch operation was successful and the game has been updated.
            // The mapper converts the updated Game entity back to a GameDto for the response.
            // This ensures a clean API boundary and separation of concerns by returning only the necessary data.
            return Ok(mapper.Map<GameDto>(gameEntity));
        }

        #endregion

        #region POST api/tournamentDetails/1/Games

        // POST: api/tournamentDetails/1/Games
        [HttpPost]
        public async Task<ActionResult<GameDto>> PostGame([FromBody] GameCreateDto gameCreateDto, [FromRoute] int tournamentId)
        {
            #region Validation of Input Parameters
            if(gameCreateDto == null) {
                // Return 400 Bad Request if the request body is null
                return BadRequest("Game data must be provided.");
            }

            // Ensure the tournamentEntity ID is set in the DTO, because the tournamentEntity Id is in the route.
            //gameCreateDto.TournamentDetailsId = tournamentId;

            // Trim whitespace from the gameEntity name
            gameCreateDto.Name = gameCreateDto.Name.Trim();
            if(string.IsNullOrWhiteSpace(gameCreateDto.Name)) {
                // Return 400 Bad Request if the game name is empty or whitespace.
                return BadRequest("Game name is required.");
            }
            #endregion

            #region Validation of Tournament existence and Game time.
            // Map the DTO to the TournamentDto
            TournamentDetails? existingTournament = await uoW.TournamentDetailsRepository.GetAsync(tournamentId);
            // Validate tournamentEntity existence
            if(existingTournament == null) {
                // Return 404 Not Found if the tournamentEntity does not exist
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            // Map the existing tournamentEntity to a TournamentDto for validation.
            TournamentDto tournamentDetailsDto = mapper.Map<TournamentDto>(existingTournament);
            // Validate that the game time is within the tournamentEntity's start and end dates.
            if(!IsGameTimeValid(gameCreateDto.Time, tournamentDetailsDto)) {
                // Return 400 Bad Request if the game time is not within the tournamentEntity's start and end dates.
                return BadRequest($"Game time must be within the tournamentEntity's start \"{tournamentDetailsDto.StartDate}\" and end \"{tournamentDetailsDto.EndDate}\" dates.");
            }
            #endregion

            #region Validation of Duplicate Games.
            // Game duplicateGame = await FindDuplicateGameAsync(gameCreateDto, tournamentId);
            // Optional: check if a similar gameEntity already exists.
            Game? duplicateGame = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(gameCreateDto.Name, tournamentId);

            if(duplicateGame != null) {
                // Return 409 Conflict if a duplicate gameEntity is found
                //return Conflict($"A gameEntity with the same name \"{gameCreateDto.Name}\" and date already exists.");
                return Conflict($"A game named \"{gameCreateDto.Name}\" already exists on {gameCreateDto.Time:yyyy-MM-dd HH:mm}.");
            }
            #endregion

            // This mapping will convert the DTO properties to the corresponding Game entity properties.
            // The Game entity will be used to persist the new game to the database.
            // This is necessary to ensure that the game is associated with the correct tournamentEntity.
            Game gameEntity = mapper.Map<Game>(gameCreateDto); // map DTO to Game entity
            gameEntity.TournamentDetailsId = tournamentId; // Set the tournamentEntity ID for the new game


            // Add the new Game entity to the repository
            // This will prepare the entity to be inserted into the database.
            // The Unit of Work pattern is used to manage the database context and repositories.
            // This ensures that the game is added to the correct tournamentEntity.
            uoW.GameRepository.Add(gameEntity);

            // Persist the changes to the database
            var changes  = await uoW.CompleteAsync();
            if(changes == 0) {
                // No changes saved - something went wrong
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save the new gameEntity.");
            }


            // Return 201 Created with the location header pointing to access the new resource.
            //nameof(GetGame): Specifies the action method (e.g., GetGame(int id)) that can be used to retrieve the created Game.
            //new { id = gameEntity.Id }: Supplies route values for the URL generation — here, it uses the newly created game’s ID.
            //mapper.Map<GameDto>(gameEntity): Returns the newly created game data in DTO form as the response body.
            return CreatedAtAction(nameof(GetGameById), new { tournamentId = gameEntity.TournamentDetailsId, id = gameEntity.Id }, mapper.Map<GameDto>(gameEntity));
        }

        #endregion

        #region DELETE api/tournamentDetails/1/Games/5


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteGame(int id, int tournamentId)
        {
            #region Validation of Input Parameters
            // Validate the ID to ensure it is a positive integer.
            // If the ID is less than or equal to zero, return 400 Bad Request.
            if(tournamentId <= 0) {
                return BadRequest($"Invalid tournamentEntity Id.");
            }

            if(id <= 0) {
                return BadRequest($"Invalid game Id.");
            }
            #endregion

            #region Validation of Tournament and Game existence.
            // Attempt to retrieve the TournamentDetails entity by ID from the repository
            TournamentDetails? tournamentEntity= await uoW.TournamentDetailsRepository.GetAsync(tournamentId);

            if(tournamentEntity == null) {
                return NotFound("Tournament with the specified ID was not found.");
            }

            // Attempt to retrieve the Game entity by ID from the repository
            Game? gameEntity = await uoW.GameRepository.GetByIdAsync(id);

            // If the entity does not exist, return 404 Not Found
            if(gameEntity == null) {
                return NotFound($"Game with the specified Id {id} was not found.");
            }

            //Make sure that the game does belong to the specified tournamentEntity.
            if(gameEntity.TournamentDetailsId != tournamentId) {
                return NotFound("Game with the specified ID does not belong to the specified tournamentEntity.");
            }

            #endregion

            // Remove the Game entity from the repository
            uoW.GameRepository.Remove(gameEntity);
            // Persist the change to the database
            await uoW.CompleteAsync();

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
        /// Validates whether the specified game time falls within the start and end dates of the tournamentEntity.
        /// </summary>
        /// <param name="gameTime">The time of the game to validate.</param>
        /// <param name="tournament">The tournamentEntity against which the game time is validated.</param>
        /// <returns>
        /// <c>true</c> if the game time is strictly between the tournamentEntity's start and end dates; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method ensures that the game is scheduled within the tournamentEntity's valid timeframe.
        /// </remarks>
        private bool IsGameTimeValid(DateTime gameTime, TournamentDto tournament)
        {
            return gameTime >= tournament.StartDate && gameTime <= tournament.EndDate;
        }

        #endregion
    }
}
