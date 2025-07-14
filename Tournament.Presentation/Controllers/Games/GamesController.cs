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

            // The GetAllAsync method returns a tuple containing the game details and meta-data for pagination.
            //(IEnumerable<GameDto> gameDetails, MetaData metaData) = await serviceManager
            //    .GameService
            //    .GetAllAsync(requestParameters, tournamentId);

            (ApiBaseResponse gameResponse, MetaData metaData) = await serviceManager.GameService.GetGamesAsync(requestParameters, tournamentId);

            //if(gameDetails is null || !gameDetails.Any()) {
            //    // If no games are found, return 404 Not Found with a message
            //    return NotFound($"No games found for tournamentEntity with ID {tournamentId}.");
            //}

            // Return the result with HTTP 200 OK
            //return Ok(games);

            if(gameResponse.Success is false) {
                return ProcessError(gameResponse);
            }

            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(metaData));

            return Ok(gameResponse.GetOkResult<IEnumerable<GameDto>>());
            //return Ok(gameDetails);
        }



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
            //GameDto gameDto = await serviceManager.GameService.GetAsync(tournamentId, trimmedTitle);
            ApiBaseResponse response = await serviceManager.GameService.GetGameAsync(tournamentId, trimmedTitle);

            //// If the gameDto is null, it means no game with the specified title exists in the tournamentEntity.
            //if(gameDto == null) {
            //    // Return 404 Not Found.
            //    return NotFound($"Game with title '{trimmedTitle}' was not found in Tournament with ID {tournamentId}.");
            //}

            return response.Success ? Ok(response.GetOkResult<GameDto>()) : ProcessError(response);

            //// Return the GameDto with HTTP 200 OK.
            //return Ok(gameDto);

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

        [Consumes("application/json-patch+json")]//Make sure your controller/method allows only application/json-patch+json to improve client request correctness.
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchGame(int tournamentId, int id, [FromBody] JsonPatchDocument<GameDto> patchDocument)
        {
            //Early validation.
            var errorResult = ValidatePatchRequest(tournamentId, id, patchDocument);
            if(errorResult is not null) {
                return errorResult;
            }

            //Get target Game.
            ApiBaseResponse gameResponse = await serviceManager.GameService.GetGameAsync(tournamentId, id);

            // If the game with the specified ID does not exist, return 404 Not Found.
            if(!gameResponse.Success) {
                return ProcessError(gameResponse);
            }

            // Apply the patch to the DTO
            GameDto patchedDto = gameResponse.GetOkResult<GameDto>();
            patchDocument.ApplyTo(patchedDto, ModelState);

            // Check for error while applying the patch
            TryValidatePatchedGame(patchedDto);
            if(!ModelState.IsValid) {
                return UnprocessableEntity(ModelState);
            }

            TournamentDto? tournamentDto = await serviceManager.TournamentService.GetByIdAsync(tournamentId);

            if(tournamentDto is null) {
                return NotFound($"Tournament with ID {tournamentId} not found.");
            }

            ApplyPatchResult result = await serviceManager.GameService.ApplyToAsync(tournamentId, id, patchedDto, tournamentDto);


            return result switch
            {
                ApplyPatchResult.InvalidDateRange => BadRequest("Game start date must be within the tournament period."),
                ApplyPatchResult.NoChanges => StatusCode(500, "Update failed. No changes were saved."),
                ApplyPatchResult.Success => NoContent(),
                _ => StatusCode(500, "Unexpected error occurred.")
            };
        }

        private bool TryValidatePatchedGame(GameDto dto)
        {
            //if(string.IsNullOrWhiteSpace(dto.Title))
            //    ModelState.AddModelError(nameof(dto.Title), "Game title is required.");

            //if(dto.StartDate == default)
            //    ModelState.AddModelError(nameof(dto.StartDate), "Start date is required.");
            ModelState.Clear();

            return TryValidateModel(dto, prefix: string.Empty);
        }
        private IActionResult? ValidatePatchRequest(int tournamentId, int id, JsonPatchDocument<GameDto>? patchDocument)
        {
            if(patchDocument is null) {
                return BadRequest("Patch document cannot be null.");
            }

            if(patchDocument.Operations is null || !patchDocument.Operations.Any()) {
                return BadRequest("Patch document must contain at least one operation.");
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

            return null;
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
