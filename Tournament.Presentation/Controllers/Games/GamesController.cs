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

            (ApiBaseResponse gameResponse, MetaData metaData) = await serviceManager.GameService.GetGamesAsync(requestParameters, tournamentId);

            if(!gameResponse.Success) {
                return ProcessError(gameResponse);
            }

            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(metaData));

            return Ok(gameResponse.GetOkResult<IEnumerable<GameDto>>());
        }

        // GET api/tournamentDetails/{tournamentId}/games/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GameDto>> GetGameById(int tournamentId, int id)
        {

            // Validate the tournamentEntity ID and game ID
            if(id <= 0 || tournamentId <= 0) {
                return BadRequest("Invalid tournamentEntity id or game id.");//
            }

            // Check if the tournamentEntity exists.
            if(!await serviceManager.TournamentService.ExistsAsync(tournamentId)) {
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            // Map the Game entity to a GameDto using AutoMapper
            ApiBaseResponse response = await serviceManager.GameService.GetGameAsync(tournamentId, id);

            // If the gameDto is null, it means the game was not found in the specified tournamentEntity
            return response.Success ? Ok(response.GetOkResult<GameDto>()) : ProcessError(response);//
        }


        // GET api/tournamentDetails/{tournamentId}/games/byTitle/{title}
        [HttpGet("byTitle/{title}")]
        public async Task<ActionResult<GameDto>> GetGameByTitle(int tournamentId, string title)
        {
            // Validation of input parameters
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

            // The GameService.GetAsync method internally verifies the tournament exists.
            ApiBaseResponse response = await serviceManager.GameService.GetGameAsync(tournamentId, trimmedTitle);

            return response.Success ? Ok(response.GetOkResult<GameDto>()) : ProcessError(response);

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

            //Get the Game.
            ApiBaseResponse gameResponse = await serviceManager.GameService.GetGameAsync(tournamentId, id);

            // If the game with the specified ID does not exist, return 404 Not Found.
            if(!gameResponse.Success) {
                return ProcessError(gameResponse);
            }

            // Apply the patch to the DTO
            GameDto patchedDto = gameResponse.GetOkResult<GameDto>();
            patchDocument.ApplyTo(patchedDto, ModelState);

            // Check for error while applying the patch

            if(!TryValidatePatchedGame(patchedDto)
             || !ModelState.IsValid) {
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
                ApplyPatchResult.NoChanges => StatusCode(409, "Update failed. No changes were saved."),
                ApplyPatchResult.Success => NoContent(),
                ApplyPatchResult.GameNotFound => StatusCode(404, "Game not found."),
                _ => StatusCode(500, "Unexpected error occurred.")
            };
        }

        private bool TryValidatePatchedGame(GameDto dto)
        {
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
                return BadRequest("Invalid tournament id.");
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
            //Validate input Parameters
            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            // Trim whitespace from the gameEntity name
            gameCreateDto.Name = gameCreateDto.Name.Trim();

            // Validate tournamentEntity existence
            if(!await serviceManager.TournamentService.ExistsAsync(tournamentId)) {
                // Return 404 Not Found if the tournamentEntity does not exist
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            //(bool isSuccess, bool isDuplicate, GameDto? gameDto) = await serviceManager.GameService.AddAsync(gameCreateDto, tournamentId);
            ApiBaseResponse response = await serviceManager.GameService.AddAsync(gameCreateDto, tournamentId);
            return response.Success ?
                CreatedAtAction(nameof(GetGameByTitle),
                new { tournamentId, title = response.GetOkResult<GameDto>().Title },
                response.GetOkResult<GameDto>()) :
                ProcessError(response);

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


    }
}
