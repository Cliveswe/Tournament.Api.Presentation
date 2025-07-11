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
using Domain.Contracts;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Tournaments.Shared.Dto;

namespace Tournaments.Presentation.Controllers.Games
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
    public class GamesController(IServiceManager serviceManager, IMapper mapper, IUnitOfWork uoW) : ControllerBase
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
        public async Task<ActionResult<IEnumerable<GameDto>>> GetTournamentGames(int tournamentId)
        {

            #region Validation of input parameters

            // Validate the tournamentEntity ID from the route parameter
            if(tournamentId <= 0) {
                // If the tournamentEntity ID is invalid (less than or equal to zero), return 400 Bad Request.
                // This ensures that the client must provide a valid tournamentEntity ID to retrieve games.
                return BadRequest($"Invalid tournamentEntity ID {tournamentId}.");
            }

            // Check if the tournamentEntity with the specified ID exists
            bool exists = await serviceManager.TournamentService.ExistsAsync(tournamentId);

            // If the tournamentEntity with the specified ID does not exist, return 404 Not Found
            if(!exists) {
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            #endregion

            // Retrieve all games associated with the specified tournamentEntity ID.
            IEnumerable<GameDto> games = await serviceManager.GameService.GetAllAsync(tournamentId);

            if(!games.Any()) {
                // If no games are found, return 404 Not Found with a message
                return NotFound($"No games found for tournamentEntity with ID {tournamentId}.");
            }

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

            #region Validation of Tournament existence, return 404 Not Found

            // Check if the tournamentEntity exists.
            if(!await serviceManager.TournamentService.ExistsAsync(tournamentId)) {
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            #endregion 

            // Map the Game entity to a GameDto using AutoMapper
            var gameDto = await serviceManager.GameService.GetAsync(tournamentId, id);

            // If the gameDto is null, it means the game was not found in the specified tournamentEntity
            if(gameDto == null) {
                return NotFound($"Game with ID {id} was not found in Tournament with ID {tournamentId}.");
            }

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
            #region Validation of input parameters
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

            // The GameService.GetAsync method internally verifies the tournament exists.
            GameDto gameDto = await serviceManager.GameService.GetAsync(tournamentId, trimmedTitle);

            // If the gameDto is null, it means no game with the specified title exists in the tournamentEntity.
            if(gameDto == null) {
                // Return 404 Not Found.
                return NotFound($"Game with title '{trimmedTitle}' was not found in Tournament with ID {tournamentId}.");
            }
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
        public async Task<ActionResult<GameDto>> PostGame([FromBody] GameCreateDto gameCreateDto, int tournamentId)
        {
            #region Validation of Input Parameters

            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            // Trim whitespace from the gameEntity name
            gameCreateDto.Name = gameCreateDto.Name.Trim();

            #endregion

            #region Validation of Tournament existence and Game time.

            // Validate tournamentEntity existence
            if(!await serviceManager.TournamentService.ExistsAsync(tournamentId)) {
                // Return 404 Not Found if the tournamentEntity does not exist
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            #endregion

            (bool isSuccess, bool isDuplicate, GameDto? gameDto) = await serviceManager.GameService.AddAsync(gameCreateDto, tournamentId);

            if(isDuplicate) {
                // If the game already exists, return 409 Conflict
                return Conflict($"Game with title '{gameCreateDto.Name}' already exists in Tournament {tournamentId}.");
            }

            if(!isSuccess) {
                // No changes saved - something went wrong
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save the new game.");
            }

            // Return 201 Created with the location header pointing to access the new resource.
            return CreatedAtAction(nameof(GetGameByTitle), new { tournamentId, title = gameDto?.Title }, gameDto);
        }

        #endregion

        #region DELETE api/tournamentDetails/1/Games/5

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteGame(int id, int tournamentId)
        {
            #region Validation of Input Parameters

            // Validate the ID to ensure it is a positive integer.
            // If the ID is less than or equal to zero, return 400 Bad Request.
            if(tournamentId <= 0 || id <= 0) {
                return BadRequest("Invalid tournamentEntity id or game id.");
            }

            #endregion

            #region Validation of Tournament and Game existence.

            // If the tournamentEntity does not exist.
            if(!await serviceManager.TournamentService.ExistsAsync(tournamentId)) {
                // Return 404 Not Found
                return NotFound("Tournament with the specified ID was not found.");
            }

            // If the game does not exist. 
            if(!await serviceManager.GameService.ExistsAsync(id)) {
                // Return 404 Not Found
                return NotFound($"Game with ID {id} was not found.");
            }

            #endregion

            // If the game does not exist or does not belong to the specified tournamentEntity.
            if(!await serviceManager.GameService.RemoveAsync(tournamentId, id)) {
                // Return 404 Not Found
                return NotFound($"Game with ID {id} in Tournament {tournamentId} was not found.");
            }

            // Return 200 OK with a confirmation message
            return Ok($"Game with ID {id} has bee deleted successfully.");
        }

        #endregion

        #region Private Methods

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
