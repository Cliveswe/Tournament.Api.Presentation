// Ignore Spelling: Api Dto Ok json

// -------------------------------------------------------------------------------------
// File: TournamentDetailsController.cs
// Summary: Handles HTTP API requests related to tournament details,
//          including retrieving, creating, updating, patching, and deleting tournaments.
//          Uses AutoMapper for DTO mapping and a Unit of Work pattern for data access.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Implements RESTful endpoints with proper status codes and error handling,
//        including partial updates via JSON Patch.
// -------------------------------------------------------------------------------------

using Domain.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Text.Json;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Tournaments.Presentation.Controllers.Tournaments
{

    [Route("api/tournamentDetails")]
    [ApiController]
    [Produces("application/json")] // Ensures all responses are documented as JSON
    public class TournamentDetailsController(IServiceManager serviceManager) : ApiControllerBase
    {
        #region GET api/TournamentDetails/5

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiBaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TournamentDto>>> GetTournamentDetails([FromQuery] TournamentRequestParameters requestParameters)
        {
            // Fetch all tournaments using the service manager
            (ApiBaseResponse tournamentResponse, MetaData metaData) = await serviceManager
                .TournamentService
                .GetAllAsync(requestParameters);

            // If no tournaments are found, return 404 Not Found
            if(!tournamentResponse.Success) {
                return ProcessError(tournamentResponse);
            }
            // If includeGames is true, we can modify the DTOs to include game details if needed.
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metaData));

            // Return the results with HTTP 200 OK
            return Ok(tournamentResponse.GetOkResult<IEnumerable<TournamentDto>>());
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TournamentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiNotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TournamentDto>> GetTournamentDetails(int id, [FromQuery] bool includeGames = false)
        {
            if(id <= 0) {
                // ID is invalid, return 400 Bad Request with an error message.
                return InvalidId(id, "Get.");
            }

            // Retrieve the tournament details by ID using the service manager
            ApiBaseResponse tournamentExists = await serviceManager
                .TournamentService
                .GetByIdAsync(id, includeGames);

            // Return 404 Not Found if the entity doesn't exist
            // Return the found entity with HTTP 200 OK + JSON by default.
            // ASP.NET Core automatically wraps it as Ok(tournamentDetails)
            return HandleResponse<TournamentDto>(tournamentExists);
        }

        #endregion

        #region PATCH api/TournamentDetails/5

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(TournamentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiNotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiUnProcessableContentResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiSaveFailedResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TournamentDto>> PatchTournament(int id, [FromBody] JsonPatchDocument<TournamentDto> patchDocument)
        {
            //Validation of Input Parameters
            if(patchDocument is null) {
                return ProcessError(new ApiBadRequestResponse("Patch document cannot be null."));
            }

            if(id <= 0) {
                return InvalidId(id, "PATCH");
            }

            // Check if tournament exists
            ApiBaseResponse? tournamentExists = await serviceManager.TournamentService.GetByIdAsync(id, trackChanges: true);

            if(!tournamentExists.Success) {
                return ProcessError(tournamentExists);
            }

            //PATCH Document and Validation
            // Apply the patch to the DTO
            patchDocument.ApplyTo(tournamentExists.GetOkResult<TournamentDto>(), ModelState);
            // Validate patched DTO
            if(!ModelState.IsValid) {
                return UnprocessableEntity(ModelState);
            }

            // Call service to update entity from patched DTO
            ApiBaseResponse updated = await serviceManager.TournamentService.ApplyToAsync(id, tournamentExists.GetOkResult<TournamentDto>());

            return HandleResponse<TournamentDto>(updated);
        }

        #endregion

        #region PUT api/TournamentDetails/5


        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiOkResponse<TournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiTournamentNotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiNoChangesMadeResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiSaveFailedResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutTournamentDetails(int id, [FromBody] TournamentUpdateDto tournamentUpdateDto)
        {

            // If the tournament does not exist.
            ApiBaseResponse entityExists = await serviceManager.TournamentService.ExistsAsync(id);
            if(!entityExists.Success) {
                // Return 404 Not Found with an error message.
                return ProcessError(entityExists);
            }

            // If the update was not successful.
            ApiBaseResponse updateResponse = await serviceManager.TournamentService.UpdateAsync(id, tournamentUpdateDto);

            // Return 404 Not Found if the tournament with the specified ID does not exist.
            // The update was successful; return HTTP 204 No Content as per REST convention
            return HandleResponse(updateResponse);

        }

        #endregion

        #region POST api/TournamentDetails

        [HttpPost]
        [ProducesResponseType(typeof(TournamentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiAlreadyExistsResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiSaveFailedResponse), StatusCodes.Status500InternalServerError)]
        // public async Task<ActionResult<TournamentDetails>> PostTournamentDetails(TournamentDetails tournamentDetails)
        public async Task<ActionResult<TournamentDto>> PostTournamentDetails([FromBody] TournamentDetailsCreateDto tournamentDetailsCreateDto)
        {
            //Validate that the tournament does not already exist
            // Check if a tournament with the same title and start date already exists.
            ApiBaseResponse tournamentExists = await serviceManager
                .TournamentService
                .ExistsAsync(tournamentDetailsCreateDto.Title, tournamentDetailsCreateDto.StartDate);

            if(tournamentExists.Success) {
                return ProcessError(new ApiAlreadyExistsResponse($"A tournament with title {tournamentDetailsCreateDto.Title} already exists."));
            }

            // Call service to create new tournament, returning tuple (id, ApiBaseResponse)
            (int id, ApiBaseResponse tournamentDto) = await serviceManager
                .TournamentService
                .CreateAsync(tournamentDetailsCreateDto);

            if(tournamentDto is null || id <= 0) {
                return ProcessError(new ApiSaveFailedResponse("Could not save the newly created tournament."));
            }

            // Return 201 Created with the route to access the new resource.
            // This follows REST conventions by providing a location header pointing to the new resource.
            return CreatedAtAction(nameof(GetTournamentDetails), new { id = id }, tournamentDto);
        }

        #endregion

        #region DELETE api/TournamentDetails/5

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiNotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiSaveFailedResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteTournamentDetails(int id)
        {
            // Validate the ID parameter
            if(id <= 0) {
                return InvalidId(id, "DELETE");
            }

            // Attempt to remove the entity from the repository.
            ApiBaseResponse deleteResponse = await serviceManager.TournamentService.RemoveAsync(id);

            // Return the deleted object wrapped in ApiOkResponse
            return HandleResponse(deleteResponse);

        }

        #endregion


        private ActionResult InvalidId(int id, string operation) =>
            ProcessError(new ApiBadRequestResponse($"Invalid tournament ID {id} specified for {operation}."));



    }
}
