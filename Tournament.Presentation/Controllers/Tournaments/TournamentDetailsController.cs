// Ignore Spelling: Api Dto Ok

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

using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;
using Tournaments.Shared.Dto;

namespace Tournaments.Presentation.Controllers.Tournaments
{

    [Route("api/tournamentDetails")]
    [ApiController]
    /// <summary>
    /// Provides RESTful API endpoints to manage tournament details, including
    /// operations to create, read, update, partially update (patch), and delete tournaments.
    /// </summary>
    /// <remarks>
    /// This controller uses the Unit of Work pattern and AutoMapper to handle
    /// data operations and DTO mappings. It supports query options such as
    /// including related games in the responses. It returns standard HTTP status
    /// codes and messages to indicate operation results.
    /// 
    /// The <see cref="PatchTournament(int, JsonPatchDocument{TournamentDto})"/> method
    /// supports partial updates on tournament entities using JSON Patch.
    /// </remarks>
    public class TournamentDetailsController(IServiceManager serviceManager, IMapper mapper, IUnitOfWork uoW) : ControllerBase
    {
        #region GET api/TournamentDetails/5

        /// <summary>
        /// Retrieves a list of all tournaments.
        /// </summary>
        /// <param name="includeGames">
        /// A query parameter indicating whether to include the related games for each tournament.
        /// If <c>true</c>, the response will include game details; otherwise, games are excluded.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ActionResult{IEnumerable{TournamentDto}}"/> containing a list of tournament data transfer objects (DTOs).
        /// </returns>
        /// <response code="200">Returns the list of tournaments (with or without games depending on <paramref name="includeGames"/>).</response>
        /// <response code="404">If no tournaments are found in the data store.</response>
        /// <remarks>
        /// This endpoint fetches tournaments asynchronously using the unit of work pattern and maps entities to DTOs using AutoMapper.
        /// If no tournaments exist, it returns a 404 Not Found with an appropriate message.
        /// </remarks>>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TournamentDto>>> GetTournamentDetails([FromQuery] bool includeGames)
        {
            // Fetch all tournaments using the service manager
            IEnumerable<TournamentDto> tournamentDetails = await serviceManager.TournamentService.GetAllAsync(includeGames);

            // If no tournaments are found, return 404 Not Found
            if(tournamentDetails == null || !tournamentDetails.Any()) {
                return NotFound("No tournaments found.");
            }

            // Return the results with HTTP 200 OK
            return Ok(tournamentDetails);
        }

        /// <summary>
        /// Retrieves the details of a specific tournament by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the tournament to retrieve.</param>
        /// <param name="includeGames">
        /// A query parameter indicating whether to include the related games for the tournament.
        /// Defaults to <c>false</c>. If <c>true</c>, the response includes game details.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ActionResult{TournamentDto}"/> containing the tournament details if found.
        /// </returns>
        /// <response code="200">Returns the tournament details (with or without games depending on <paramref name="includeGames"/>).</response>
        /// <response code="404">If no tournament with the specified <paramref name="id"/> is found.</response>
        /// <remarks>
        /// This endpoint fetches a tournament asynchronously using the unit of work pattern and maps the entity to a DTO using AutoMapper.
        /// If the tournament does not exist, it returns a 404 Not Found with an appropriate message.
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<ActionResult<TournamentDto>> GetTournamentDetails(int id, [FromQuery] bool includeGames = false)
        {
            if(id <= 0) {
                // If the ID is invalid, return 400 Bad Request with an error message.
                return BadRequest($"Invalid tournament ID {id} specified.");
            }

            // Retrieve the tournament details by ID using the service manager
            TournamentDto? tournamentDto = await serviceManager
                .TournamentService
                .GetByIdAsync(id, includeGames);

            // Return 404 Not Found if the entity doesn't exist
            if(tournamentDto == null) {
                return NotFound($"Tournament with ID {id} was not found.");
            }

            // Return the found entity with HTTP 200 OK + JSON by default.
            // ASP.NET Core automatically wraps it as Ok(tournamentDetails)
            return Ok(tournamentDto);
        }

        #endregion

        #region PATCH api/TournamentDetails/5

        /// <summary>
        /// Partially updates an existing tournament identified by <paramref name="tournamentId"/> using a JSON Patch document.
        /// </summary>
        /// <param name="tournamentId">The ID of the tournament to be patched. Must be greater than zero.</param>
        /// <param name="patchDocument">A <see cref="JsonPatchDocument{TournamentDto}"/> representing the JSON Patch operations to apply to the tournament DTO.</param>
        /// <returns>
        /// Returns an <see cref="ActionResult{TournamentDto}"/> containing the updated tournament data if the patch succeeds.
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="number">
        /// <item>Validates that the patch document is not null.</item>
        /// <item>Validates that the tournament ID is valid (greater than zero).</item>
        /// <item>Retrieves the existing tournament entity from the repository via the Unit of Work pattern.</item>
        /// <item>If the tournament does not exist, returns 404 Not Found.</item>
        /// <item>Maps the existing tournament entity to a <see cref="TournamentDto"/> for patching.</item>
        /// <item>Applies the patch document to the DTO and validates the patched model state.</item>
        /// <item>If validation fails, returns 400 Bad Request with validation errors.</item>
        /// <item>Maps the patched DTO back to the entity and updates it in the repository.</item>
        /// <item>Handles concurrency exceptions and checks if the tournament still exists.</item>
        /// <item>Persists changes via Unit of Work.</item>
        /// <item>Returns the updated tournament DTO with HTTP 200 OK on success.</item>
        /// </list>
        /// </remarks>
        /// <exception cref="DbUpdateConcurrencyException">Thrown if a concurrency conflict occurs during update and the tournament still exists.</exception>
        [HttpPatch("{tournamentId}")]
        public async Task<ActionResult<TournamentDto>> PatchTournament(int tournamentId, JsonPatchDocument<TournamentDto> patchDocument)
        {
            #region Validation of Input Parameters
            // Validate the model state, checks data annotations.
            if(patchDocument == null) {
                // If the model state is invalid, return 400 Bad Request with validation errors.
                return BadRequest("Patch document cannot be null.");
            }

            // Check if the tournament ID is valid.
            if(tournamentId <= 0) {
                // If the tournament ID is invalid, return 400 Bad Request with an error message.
                return BadRequest($"Invalid tournament ID {tournamentId} specified for patching.");
            }

            #endregion

            //TODO: Consider using a service method to encapsulate the patch logic.

            // Retrieve the existing tournament details from the repository using the Unit of Work pattern.
            TournamentDetails? existingTournament = await uoW.TournamentDetailsRepository.GetAsync(tournamentId);

            // Check if the tournament exists.
            if(existingTournament == null) {
                // If the tournament does not exist, return 404 Not Found with an error message.
                return NotFound($"Tournament with ID {tournamentId} was not found.");
            }

            // Map the existing tournament entity to a DTO for patching.
            TournamentDto tournamentDto = mapper.Map<TournamentDto>(existingTournament);
            // Apply the patch document to the DTO, updating its properties.
            patchDocument.ApplyTo(tournamentDto, ModelState);
            // Validate the patched DTO against the model state.
            TryValidateModel(tournamentDto);

            // Validate the patched model state after applying the patch document.
            if(!ModelState.IsValid) {
                // If the model state is invalid after patching, return 400 Bad Request with validation errors.
                return BadRequest(ModelState);
            }

            // Attempt to update the game in the repository
            try {
                // Map the patched DTO back to the existing tournament entity.
                _ = mapper.Map(tournamentDto, existingTournament);// This updates only the fields specified in the DTO.
                // Attempt to update the existing tournament entity in the repository.
                uoW.TournamentDetailsRepository.Update(existingTournament);
                // Persist the changes to the database using the Unit of Work pattern.
                await uoW.CompleteAsync();
            } catch(DbUpdateConcurrencyException) {
                // Check if the tournament still exists after the concurrency exception.
                bool exists = await TournamentDetailsExists(existingTournament.Id);
                if(!exists) {
                    // If the tournament no longer exists, return 404 Not Found with an error message.
                    return NotFound($"Tournament with ID {tournamentId} was not found. It may have been deleted or does not exist.");
                } else {
                    throw;
                }
            }

            // Return the updated tournament details with HTTP 200 OK.
            // The mapper converts the updated entity back to a DTO for the response.
            // This ensures a clean API boundary by returning only the necessary data.
            return Ok(mapper.Map<TournamentDto>(existingTournament));
        }

        #endregion

        #region PUT api/TournamentDetails/5

        /// <summary>
        /// Updates an existing tournament's details identified by the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the tournament to update.</param>
        /// <param name="tournamentUpdateDto">The data transfer object containing the updated tournament information.</param>
        /// <returns>
        /// Returns 204 No Content if the update is successful.
        /// Returns 400 Bad Request if the model state is invalid or the input DTO is null.
        /// Returns 404 Not Found if no tournament with the specified ID exists.
        /// </returns>
        /// <remarks>
        /// This method validates the incoming model, retrieves the existing tournament entity,
        /// maps the updated fields from the DTO, and attempts to save the changes.
        /// It handles concurrency exceptions and ensures RESTful response codes are returned.
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTournamentDetails(int id, [FromBody] TournamentUpdateDto tournamentUpdateDto)
        {
            #region Validation of Input Parameters

            // Validate the model state, checks data annotations.
            if(!ModelState.IsValid) {
                // Return 400 Bad Request with validation errors
                return BadRequest(ModelState);
            }

            // Check for null input.
            if(tournamentUpdateDto == null) {
                return BadRequest("Update data cannot be null.");
            }

            #endregion

            // If the tournament does not exist.
            if(!await serviceManager.TournamentService.ExistsAsync(id)) {
                // Return 404 Not Found with an error message.
                return NotFound($"Tournament with ID {id} was not found.");
            }

            // If the update was not successful.
            if(!await serviceManager.TournamentService.Update(id, tournamentUpdateDto)) {
                // Return 404 Not Found if the tournament with the specified ID does not exist.
                return NotFound($"Tournament with ID {id} was not found.");
            }

            // The update was successful; return HTTP 204 No Content as per REST convention
            return NoContent();
        }

        #endregion

        #region POST api/TournamentDetails

        /// <summary>
        /// Creates a new tournament record in the data store.
        /// </summary>
        /// <param name="tournamentDetailsCreateDto">The data transfer object containing the details of the tournament to create.</param>
        /// <returns>
        /// Returns a <see cref="CreatedAtActionResult"/> containing the newly created tournament data with a 201 Created status code.
        /// Returns 400 Bad Request if the input model is invalid or null.
        /// Returns 409 Conflict if a tournament with the same title and start date already exists.
        /// </returns>
        /// <remarks>
        /// This method validates the input model, checks for duplicates to prevent conflicts,
        /// maps the DTO to the domain entity, adds the new entity to the repository,
        /// and persists changes asynchronously. It follows RESTful conventions by returning
        /// the location of the newly created resource in the response.
        /// </remarks>
        [HttpPost]
        // public async Task<ActionResult<TournamentDetails>> PostTournamentDetails(TournamentDetails tournamentDetails)
        public async Task<ActionResult<TournamentDetails>> PostTournamentDetails(TournamentDetailsCreateDto tournamentDetailsCreateDto)
        {
            #region Validation of Input Parameters

            // Validate the model stat, checks data annotations.
            if(!ModelState.IsValid) {
                // Return 400 Bad Request with validation errors
                return BadRequest(ModelState);
            }

            // Check for null input.
            if(tournamentDetailsCreateDto == null) {
                return BadRequest("TournamentDetails cannot be null.");
            }

            #endregion

            #region Validate that the tournament does not already exist

            // Check if a tournament with the same title and start date already exists.
            bool exists = await serviceManager
                .TournamentService
                .ExistsAsync(tournamentDetailsCreateDto.Title, tournamentDetailsCreateDto.StartDate);

            if(exists) {
                return Conflict($"A tournament with the same name \"{tournamentDetailsCreateDto.Title}\" and start date already exists.");
            }

            #endregion

            // This returns a tuple containing the ID of the newly created tournament and the mapped DTO.
            (int id, TournamentDto tournamentDto) = await serviceManager
                .TournamentService
                .CreateAsync(tournamentDetailsCreateDto);

            // Return 201 Created with the route to access the new resource.
            // This follows REST conventions by providing a location header pointing to the new resource.
            return CreatedAtAction(nameof(GetTournamentDetails), new { id = id }, tournamentDto);
        }

        #endregion

        #region DELETE api/TournamentDetails/5

        /// <summary>
        /// Deletes a tournament with the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the tournament to delete. Must be greater than zero.</param>
        /// <returns>
        /// Returns 200 OK with a success message if the tournament was deleted successfully.
        /// Returns 400 Bad Request if the provided ID is invalid (less than or equal to zero).
        /// Returns 404 Not Found if a tournament with the specified ID does not exist.
        /// Returns 500 Internal Server Error if an error occurs during deletion.
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTournamentDetails(int id)
        {
            // Validate the ID parameter
            if(id <= 0) {
                return BadRequest($"Invalid {id} specified for deletion.");
            }

            // Attempt to remove the entity from the repository.
            if(!await serviceManager.TournamentService.RemoveAsync(id)) {
                // If the removal was not successful, return 404 Not Found
                return NotFound($"Tournament with ID {id} was not found.");
            }

            // Return 200 OK with a success message
            return Ok(new { message = $"Tournament with ID {id} has been deleted successfully." });

        }

        #endregion

        #region private methods

        /// <summary>
        /// Checks asynchronously whether a <see cref="TournamentDetails"/> entity exists with the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the tournament to check for existence.</param>
        /// <returns>
        /// Returns <c>true</c> if a tournament with the given ID exists; otherwise, <c>false</c>.
        /// </returns>
        private async Task<bool> TournamentDetailsExists(int id)
        {
            // Check for existence using the Game ID
            // Returns true if a Game with the same ID exists, otherwise false
            return await uoW.TournamentDetailsRepository.AnyAsync(id);

            // Alternatively, using direct DbContext access:
            //return context.TournamentDetails.Any(e => e.Id == id);
        }

        #endregion
    }
}
