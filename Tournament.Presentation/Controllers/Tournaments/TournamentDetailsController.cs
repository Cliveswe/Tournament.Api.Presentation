// Ignore Spelling: Api Dto Ok json

// -------------------------------------------------------------------------------------
// File: TournamentDetailsController.cs
// Summary: Handles HTTP API requests related to tournament details,
//          including retrieving, creating, updating, patching, and deleting tournaments.
//          Uses AutoMapper for DTO mapping and a Unit of Work pattern for data access.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// <lastModified> [2025-08-04] </lastModified>
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

namespace Tournaments.Presentation.Controllers.Tournaments;

/// <summary>
/// Handles HTTP API requests related to tournament details, including retrieval, creation,
/// updating, partial updates via JSON Patch, and deletion of tournament entities.
/// </summary>
/// <remarks>
/// This controller implements RESTful endpoints following standard HTTP semantics and status codes.
/// It uses dependency-injected services via <see cref="IServiceManager"/> to manage tournament operations,
/// applies DTO mapping, pagination metadata, custom error responses, and enforces JSON content-type constraints.
/// </remarks>
/// <response code="200">Successful request with data returned (GET, PUT, PATCH).</response>
/// <response code="201">Resource successfully created (POST).</response>
/// <response code="204">Resource successfully deleted (DELETE).</response>
/// <response code="400">Invalid input or malformed request.</response>
/// <response code="404">Requested tournament was not found.</response>
/// <response code="409">Conflict due to an existing tournament with similar data (POST).</response>
/// <response code="422">Non-processable request due to validation or lack of changes (PUT/PATCH).</response>
/// <response code="500">Internal server error occurred during processing.</response>
[Route("api/tournamentDetails")]
[ApiController]
[Produces("application/json")] // Ensures all responses are documented as JSON
public class TournamentDetailsController(IServiceManager serviceManager) : ApiControllerBase
{
    #region GET api/TournamentDetails/5

    /// <summary>
    /// Retrieves a paginated list of tournament details based on the specified query parameters.
    /// </summary>
    /// <param name="requestParameters">Filtering, sorting, and pagination parameters for tournaments.</param>
    /// <returns>An <see cref="ActionResult"/> containing a collection of <see cref="TournamentDto"/> objects
    /// if successful, or an appropriate error response.</returns>
    /// <response code="200">Returns the list of tournaments matching the criteria.</response>
    /// <response code="404">No tournaments found matching the query parameters.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
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

    /// <summary>
    /// Retrieves the details of a tournament by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the tournament to retrieve.</param>
    /// <param name="includeGames">Optional query parameter to include related games in the response.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing the <see cref="TournamentDto"/> if found,
    /// or an error response indicating why the tournament could not be retrieved.
    /// </returns>
    /// <response code="200">Returns the tournament details.</response>
    /// <response code="400">If the provided tournament ID is invalid.</response>
    /// <response code="404">If the tournament with the specified ID is not found.</response>
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

    /// <summary>
    /// Applies a JSON Patch document to partially update an existing tournament.
    /// </summary>
    /// <param name="id">The ID of the tournament to update.</param>
    /// <param name="patchDocument">The JSON Patch document containing the changes.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing the updated <see cref="TournamentDto"/> if successful,
    /// or an error response if the patch is invalid, the tournament is not found, or the update fails.
    /// </returns>
    /// <response code="200">Returns the updated tournament details.</response>
    /// <response code="400">If the patch document is null or the ID is invalid.</response>
    /// <response code="404">If the tournament with the specified ID is not found.</response>
    /// <response code="422">If the patch document results in validation errors.</response>
    /// <response code="500">If there is a failure saving the updated tournament.</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(TournamentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiNotFoundResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiUnProcessableContentResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiSaveFailedResponse), StatusCodes.Status500InternalServerError)]
    [Consumes("application/json-patch+json")] // PATCH consumes JSON Patch content type
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

    /// <summary>
    /// Updates the details of an existing tournament identified by its ID.
    /// </summary>
    /// <param name="id">The ID of the tournament to update.</param>
    /// <param name="tournamentUpdateDto">The updated tournament data transfer object.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> indicating the result of the update operation.
    /// Returns the updated tournament details on success.
    /// </returns>
    /// <response code="200">The tournament was successfully updated and the updated data is returned.</response>
    /// <response code="400">The request was invalid, such as invalid input data.</response>
    /// <response code="404">The tournament with the specified ID was not found.</response>
    /// <response code="422">No changes were made during the update.</response>
    /// <response code="500">An error occurred while saving the updated tournament.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiOkResponse<TournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBadRequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiTournamentNotFoundResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiNoChangesMadeResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiSaveFailedResponse), StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")] // PUT consumes normal JSON payloads
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

    /// <summary>
    /// Creates a new tournament with the provided tournament details.
    /// </summary>
    /// <param name="tournamentDetailsCreateDto">The DTO containing details of the tournament to be created.</param>
    /// <returns>
    /// An <see cref="ActionResult{TournamentDto}"/> containing the created tournament if successful,
    /// or an error response if creation fails or the tournament already exists.
    /// </returns>
    /// <response code="201">Returns the created tournament with a location header.</response>
    /// <response code="409">If a tournament with the same title and start date already exists.</response>
    /// <response code="500">If an error occurs while saving the tournament.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TournamentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiAlreadyExistsResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiSaveFailedResponse), StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")] // Correct MIME type for POSTing a DTO
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
        return CreatedAtAction(nameof(GetTournamentDetails), new { id = id }, tournamentDto.GetOkResult<TournamentDto>());
    }

    #endregion

    #region DELETE api/TournamentDetails/5

    /// <summary>
    /// Deletes an existing tournament identified by its ID.
    /// </summary>
    /// <param name="id">The ID of the tournament to delete.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> indicating the result of the delete operation.
    /// Returns <c>204 No Content</c> if the tournament is successfully deleted.
    /// </returns>
    /// <response code="204">The tournament was successfully deleted.</response>
    /// <response code="400">The ID provided is invalid.</response>
    /// <response code="404">The tournament with the specified ID was not found.</response>
    /// <response code="500">An error occurred while deleting the tournament.</response>
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

    /// <summary>
    /// Generates a standardized <see cref="ApiBadRequestResponse"/> for invalid tournament ID inputs.
    /// </summary>
    /// <param name="id">The invalid tournament ID that was provided.</param>
    /// <param name="operation">The name of the operation (e.g., GET, DELETE) for which the ID was used.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing a <see cref="ApiBadRequestResponse"/> indicating the ID is invalid.
    /// </returns>
    private ActionResult InvalidId(int id, string operation) =>
        ProcessError(new ApiBadRequestResponse($"Invalid tournament ID {id} specified for {operation}."));

}
