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
using Domain.Models.Entities;
using Domain.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Service.Contracts.Enums;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

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
    public class GamesController(IServiceManager serviceManager) : ApiControllerBase
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
        //public async Task<ActionResult<IEnumerable<GameDto>>> GetTournamentGames(int tournamentId)
        public async Task<ActionResult<IEnumerable<GameDto>>> GetTournamentGames([FromQuery] TournamentRequestParameters requestParameters, int tournamentId)
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

            // The GetAllAsync method returns a tuple containing the game details and metadata for pagination.
            (IEnumerable<GameDto> gameDetails, MetaData metaData) = await serviceManager
                .GameService
                .GetAllAsync(requestParameters, tournamentId);

            if(gameDetails is null || !gameDetails.Any()) {
                // If no games are found, return 404 Not Found with a message
                return NotFound($"No games found for tournamentEntity with ID {tournamentId}.");
            }

            // Return the result with HTTP 200 OK
            //return Ok(games);

            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(metaData));

            return Ok(gameDetails);
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
            //var gameDto = await serviceManager.GameService.GetAsync(tournamentId, id);
            ApiBaseResponse response = await serviceManager.GameService.GetGameAsync(tournamentId, id);

            // If the gameDto is null, it means the game was not found in the specified tournamentEntity
            //if(gameDto == null) {
            //    return NotFound($"Game with ID {id} was not found in Tournament with ID {tournamentId}.");
            //}
            return response.Success ? Ok(response.GetOkResult<GameDto>()) : ProcessError(response);

            // Return the GameDto with HTTP 200 OK
            //return Ok(gameDto);
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

            // The GameService.UpdateAsync method internally verifies the tournament exists and the game title matches.
            UpdateGameResult result = await serviceManager.GameService.UpdateAsync(tournamentId, title, gameUpdateDto);

            // Check the result of the update operation
            return result switch
            {
                UpdateGameResult.NotFound => NotFound($"Game with title '{title}' was not found."),
                UpdateGameResult.NotModified => StatusCode(500, "Update failed. No changes were saved."),
                UpdateGameResult.Success => NoContent(),
                _ => StatusCode(500, "Unexpected update result.")
            };
        }

        #endregion

        #region PATCH api/tournamentDetails/1/Games/5

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchGame(int tournamentId, int id, [FromBody] JsonPatchDocument<GameDto> patchDocument)
        {
            #region Validation of Input Parameters

            if(patchDocument is null) {
                return BadRequest("Patch document cannot be null.");
            }

            // Validate the input parameters.
            if(!ModelState.IsValid) {
                // Return 400 Bad Request if the model state is invalid
                return BadRequest(ModelState);
            }

            if(tournamentId <= 0) {
                // If the tournament ID is invalid (less than or equal to zero), return 400 Bad Request.
                return BadRequest("Invalid tournamentEntity id.");
            }
            // Validate the game ID from the route parameter

            if(id <= 0) {
                // If the game ID is invalid (less than or equal to zero), return 400 Bad Request.
                return BadRequest("Invalid game id.");
            }

            #endregion

            GameDto? gameDto = await serviceManager.GameService.GetAsync(tournamentId, id);

            if(gameDto is null) {
                // If the game with the specified ID does not exist, return 404 Not Found.
                return NotFound($"Game with ID {id} in Tournament {tournamentId} was not found.");
            }

            #region PATCH Document Validation

            // Apply the patch to the DTO
            patchDocument.ApplyTo(gameDto, ModelState);

            // Validate patched DTO
            if(!ModelState.IsValid) {
                return UnprocessableEntity(ModelState);
            }

            #endregion

            #region Validate Game date is within the Tournament period
            TournamentDto? tournamentDto = await serviceManager.TournamentService.GetByIdAsync(tournamentId);

            if(tournamentDto is null) {
                return NotFound($"Tournament with ID {tournamentId} not found.");
            }

            ApplyPatchResult result = await serviceManager.GameService.ApplyToAsync(tournamentId, id, gameDto, tournamentDto);

            #endregion

            return result switch
            {
                ApplyPatchResult.InvalidDateRange => BadRequest("Game start date must be within the tournament period."),
                ApplyPatchResult.NoChanges => StatusCode(500, "Update failed. No changes were saved."),
                ApplyPatchResult.Success => NoContent(),
                _ => StatusCode(500, "Unexpected error occurred.")
            };
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
