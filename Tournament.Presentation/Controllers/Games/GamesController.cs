// Ignore Spelling: Api Dto json

// --------------------------------------------------------------------------------
// File: GamesController.cs
// Summary: Handles HTTP API requests related to game entities within tournaments,
//          including retrieving, creating, updating, patching, and deleting games.
//          Utilizes AutoMapper for DTO mapping and the Unit of Work pattern for
//          reliable and efficient data access management.
// <author> [Clive Leddy] </author>
// <created> [2025-06-28] </created>
// <lastModified> [2025-08-04] </lastModified>
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
    [Produces("application/json")] // Ensures all responses are documented as JSON
    public class GamesController(IServiceManager serviceManager) : ApiControllerBase
    {
        #region GET api/Games api/1/Games/ api/1/Games/21

        /// <summary>
        /// Retrieves all games for a specified tournament with optional filtering and pagination.
        /// </summary>
        /// <param name="requestParameters">Query parameters for paging, sorting, etc.</param>
        /// <param name="tournamentId">The ID of the tournament to retrieve games from.</param>
        /// <returns>
        /// A list of <see cref="GameDto"/> wrapped in a 200 OK response if found.
        /// Returns 400 Bad Request if the tournament ID is invalid, or 404 Not Found if the tournament does not exist.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiNotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetTournamentGames([FromQuery] TournamentRequestParameters requestParameters, int tournamentId)
        {

            #region Validation of input parameters

            // Validate the tournamentEntity ID from the route parameter
            if(tournamentId <= 0) {
                // If the tournamentEntity ID is invalid (less than or equal to zero), return 400 Bad Request.
                // This ensures that the client must provide a valid tournamentEntity ID to retrieve games.
                return ProcessError(new ApiBadRequestResponse($"Invalid tournamentEntity ID {tournamentId}."));
            }

            // Check if the tournamentEntity with the specified ID exists
            ApiBaseResponse exists = await serviceManager.DoesTournamentExist(tournamentId);

            // If the tournamentEntity with the specified ID does not exist, return 404 Not Found
            if(!exists.Success) {
                return ProcessError(new ApiNotFoundResponse($"Tournament with ID {tournamentId} does not exist."));
            }

            #endregion

            (ApiBaseResponse gameResponse, MetaData metaData) = await serviceManager.GameService.GetGamesAsync(requestParameters, tournamentId);

            if(!gameResponse.Success) {
                return ProcessError(gameResponse);
            }

            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(metaData));

            return Ok(gameResponse.GetOkResult<IEnumerable<GameDto>>());
        }

        /// <summary>
        /// Retrieves a specific game by its ID within a given tournament.
        /// </summary>
        /// <param name="tournamentId">The ID of the tournament containing the game.</param>
        /// <param name="id">The ID of the game to retrieve.</param>
        /// <returns>
        /// Returns a <see cref="GameDto"/> with HTTP 200 OK if found; otherwise, returns an appropriate error response:
        /// 400 Bad Request if input IDs are invalid,
        /// 404 Not Found if the tournament or game does not exist.
        /// </returns>
        // GET api/tournamentDetails/{tournamentId}/games/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiNotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameDto>> GetGameById(int tournamentId, int id)
        {

            // Validate the tournamentEntity ID and game ID
            if(id <= 0 || tournamentId <= 0) {
                return ProcessError(new ApiBadRequestResponse("Invalid tournamentEntity id or game id."));
            }

            // Check if the tournamentEntity exists.
            ApiBaseResponse entityExists =await serviceManager.DoesTournamentExist(tournamentId);
            if(!entityExists.Success) {
                return ProcessError(new ApiNotFoundResponse($"Tournament with ID {tournamentId} does not exist."));
            }

            // Map the Game entity to a GameDto using AutoMapper
            ApiBaseResponse response = await serviceManager.GameService.GetGameAsync(tournamentId, id);

            // If the gameDto is null, it means the game was not found in the specified tournamentEntity
            return response.Success ? Ok(response.GetOkResult<GameDto>()) : ProcessError(response);//
        }

        /// <summary>
        /// Retrieves a game by its title for the specified tournament.
        /// </summary>
        /// <param name="tournamentId">The ID of the tournament containing the game.</param>
        /// <param name="title">The title of the game to retrieve.</param>
        /// <returns>
        /// A <see cref="GameDto"/> object if found, otherwise a relevant error response.
        /// </returns>
        [HttpGet("title/{title}")]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiGameNotFoundByTitleResponse), StatusCodes.Status404NotFound)]
        // GET api/tournamentDetails/{tournamentId}/games/byTitle/{title}
        public async Task<ActionResult<GameDto>> GetGameByTitle(int tournamentId, string title)
        {
            // Validation of input parameters
            if(string.IsNullOrWhiteSpace(title)) {
                return ProcessError(new ApiBadRequestResponse("Title must be a non-empty string."));
            }

            // Trim whitespace from the title to ensure accurate matching.
            string trimmedTitle = title.Trim();

            // Validate the tournamentEntity ID.
            if(tournamentId <= 0) {
                return ProcessError(new ApiBadRequestResponse("Invalid tournamentEntity id."));
            }

            // The GameService.GetAsync method internally verifies the tournament exists.
            ApiBaseResponse response = await serviceManager.GameService.GetGameAsync(tournamentId, trimmedTitle);

            return response.Success ? Ok(response.GetOkResult<GameDto>()) : ProcessError(response);

        }

        #endregion

        #region PUT api/tournamentDetails/1/Games/5


        /// <summary>
        /// Updates an existing game identified by title within a specific tournament.
        /// </summary>
        /// <param name="tournamentId">The ID of the tournament that contains the game.</param>
        /// <param name="title">The title of the game to update.</param>
        /// <param name="gameUpdateDto">The update payload for the game.</param>
        /// <returns>
        /// A success response with the updated <see cref="GameDto"/>, or an error response.
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBadGamePatchDocumentResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiGameNotFoundByTitleResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiAlreadyExistsResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiNoChangesMadeResponse), StatusCodes.Status422UnprocessableEntity)]
        [Consumes("application/json-patch+json")]//Make sure your controller/method allows only application/json-patch+json to improve client request correctness.
        public async Task<ActionResult> PutGame(int tournamentId, [FromQuery] string title, [FromBody] GameUpdateDto gameUpdateDto)
        {
            // Validate the tournamentEntity ID from the route parameter.
            if(tournamentId <= 0) {
                // Return 400 Bad Request if the tournamentEntity ID is invalid
                return ProcessError(new ApiBadGamePatchDocumentResponse($"Invalid tournament {tournamentId}."));
            }

            // Validate the game title input
            title = title.Trim();
            if(string.IsNullOrWhiteSpace(title)) {
                // Return 400 Bad Request if the title ID is invalid
                return ProcessError(new ApiBadGamePatchDocumentResponse("Game title must be a non-empty string."));
            }

            // The GameService.UpdateAsync method internally verifies the tournament exists and the game title matches.
            ApiBaseResponse result = await serviceManager.GameService.UpdateAsync(tournamentId, title, gameUpdateDto);

            return result.Success ? Ok(result.GetOkResult<GameDto>()) : ProcessError(result);
        }

        #endregion

        #region PATCH api/tournamentDetails/1/Games/5

        /// <summary>
        /// Applies a JSON Patch to an existing game within a specified tournament.
        /// </summary>
        /// <param name="tournamentId">The ID of the tournament containing the game.</param>
        /// <param name="id">The ID of the game to patch.</param>
        /// <param name="patchDocument">The JSON Patch document with operations to apply.</param>
        /// <returns>A response indicating the result of the patch operation.</returns>
        [HttpPatch("{id:int}")]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBadGamePatchDocumentResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiGameNotFoundByIdResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiUnProcessableContentResponse), StatusCodes.Status422UnprocessableEntity)]
        [Consumes("application/json-patch+json")]//Make sure your controller/method allows only application/json-patch+json to improve client request correctness.
        public async Task<ActionResult> PatchGame(int tournamentId, int id, [FromBody] JsonPatchDocument<GameDto> patchDocument)
        {
            //Early validation.
            ApiBaseResponse? errorResult = ValidatePatchRequest(tournamentId, id, patchDocument);
            if(errorResult is not null) {
                return ProcessError(errorResult);
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
            if(!TryValidatePatchedGame(patchedDto) || !ModelState.IsValid) {
                //return UnprocessableEntity(ModelState);
                string errorMessage = string.Join("; ",
                    ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                ApiUnProcessableContentResponse errorResponse = new ApiUnProcessableContentResponse(errorMessage);
                return ProcessError(errorResponse);
            }

            ApiBaseResponse? tournamentDto = await serviceManager.TournamentService.GetByIdAsync(tournamentId);

            if(!tournamentDto.Success) {
                //return NotFound($"Tournament with ID {tournamentId} not found.");
                return ProcessError(
                    new ApiBadGamePatchDocumentResponse($"Tournament with ID {tournamentId} not found.")
                    );
            }

            //Apply the patched dto to the db.
            ApiBaseResponse response = await serviceManager.GameService.ApplyToAsync(tournamentId, id, patchedDto, tournamentDto.GetOkResult<TournamentDto>());
            return response.Success ? Ok(response.GetOkResult<GameDto>()) : ProcessError(response);
        }

        private bool TryValidatePatchedGame(GameDto dto)
        {
            ModelState.Clear();

            return TryValidateModel(dto, prefix: string.Empty);
        }

        private ApiBaseResponse? ValidatePatchRequest(int tournamentId, int id, JsonPatchDocument<GameDto>? patchDocument)
        {
            if(patchDocument is null) {
                return new ApiBadGamePatchDocumentResponse("Patch document cannot be null.");
            }

            if(patchDocument.Operations is null || !patchDocument.Operations.Any()) {
                return new ApiBadGamePatchDocumentResponse("Patch document must contain at least one operation.");
            }


            if(tournamentId <= 0) {
                // If the tournament ID is invalid (less than or equal to zero), return 400 Bad Request.
                return new ApiBadGamePatchDocumentResponse("Invalid tournament id.");
            }
            // Validate the game ID from the route parameter

            if(id <= 0) {
                // If the game ID is invalid (less than or equal to zero), return 400 Bad Request.
                return new ApiBadGamePatchDocumentResponse("Invalid game id.");
            }

            return null;
        }

        #endregion

        #region POST api/tournamentDetails/1/Games

        /// <summary>
        /// Creates a new game within the specified tournament.
        /// </summary>
        /// <param name="gameCreateDto">The game details to create.</param>
        /// <param name="tournamentId">The ID of the tournament where the game should be created.</param>
        /// <returns>
        /// Returns a <see cref="GameDto"/> representing the newly created game if successful.
        /// Otherwise, returns appropriate error responses for invalid input, conflicts, or failures.
        /// </returns>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiNotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiAlreadyExistsResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiSaveFailedResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GameDto>> PostGame([FromBody] GameCreateDto gameCreateDto, int tournamentId)
        {
            //Validate input Parameters
            if(tournamentId < 0) {
                return ProcessError(new ApiBadRequestResponse($"Invalid tournamentEntity ID {tournamentId}."));
            }

            // Trim whitespace from the gameEntity name
            gameCreateDto.Name = gameCreateDto.Name.Trim();

            // Validate tournamentEntity existence
            //if(!await serviceManager.TournamentService.ExistsAsync(tournamentId)) {
            ApiBaseResponse entityExists = await serviceManager.DoesTournamentExist(tournamentId);
            if(!entityExists.Success) {
                // Return 404 Not Found if the tournamentEntity does not exist
                // return NotFound($"Tournament with ID {tournamentId} does not exist.");
                return ProcessError(new ApiNotFoundResponse($"Tournament with ID {tournamentId} does not exist."));
            }

            //(bool isSuccess, bool isDuplicate, GameDto? gameDto) = await serviceManager.GameService.AddAsync(gameCreateDto, tournamentId);
            ApiBaseResponse response = await serviceManager.GameService.AddGameAsync(gameCreateDto, tournamentId);

            return response.Success ?
                CreatedAtAction(nameof(GetGameByTitle),
                new { tournamentId, title = response.GetOkResult<GameDto>().Title },
                response.GetOkResult<GameDto>()) :
                ProcessError(response);

        }

        #endregion

        #region DELETE api/tournamentDetails/1/Games/5

        /// <summary>
        /// Deletes a game by its ID from the specified tournament.
        /// </summary>
        /// <param name="id">The ID of the game to delete.</param>
        /// <param name="tournamentId">The ID of the tournament the game belongs to.</param>
        /// <returns>
        /// Returns 200 OK with a confirmation message if the deletion is successful.
        /// Returns 400 Bad Request if the IDs are invalid.
        /// Returns 404 Not Found if the tournament or game is not found.
        /// </returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiGameNotFoundByIdResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiTournamentNotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiSaveFailedResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GameDto>> DeleteGame(int id, int tournamentId)
        {

            // Validate the ID to ensure it is a positive integer.
            // If the ID is less than or equal to zero, return 400 Bad Request.
            if(tournamentId <= 0 || id <= 0) {
                return ProcessError(new ApiBadRequestResponse("Invalid tournamentEntity id or game id."));
            }

            ApiBaseResponse response = await serviceManager.GameService.RemoveAsync(tournamentId, id);

            // Return OK with a confirmation error message.
            return response.Success ? Ok(response.Message) : ProcessError(response);
        }

        #endregion


    }
}
