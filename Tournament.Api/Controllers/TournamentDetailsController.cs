// Ignore Spelling: Api Dto Ok

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Dto;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;

namespace Tournament.Api.Controllers
{

    [Route("api/tournamentDetails")]
    [ApiController]
    //public class TournamentDetailsController(TournamentApiContext context, IUoW uoW) : ControllerBase
    public class TournamentDetailsController(IMapper mapper, IUoW uoW) : ControllerBase
    {
        #region GET api/TournamentDetails api/TournamentDetails/5

        /// <summary>
        /// Retrieves all TournamentDetails entities.
        /// </summary>
        /// <param name="includeGames">Optional query parameter indicating whether to include related games.</param>
        /// <returns>A 200 OK response containing a collection of TournamentDto objects, including games if requested.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TournamentDto>>> GetTournamentDetails([FromQuery] bool includeGames)
        {
            IEnumerable<TournamentDto> tournamentDetails = includeGames
                ? mapper.Map<IEnumerable<TournamentDto>>(await uoW.TournamentDetailsRepository.GetAllAsync(includeGames))
                : mapper.Map<IEnumerable<TournamentDto>>(await uoW.TournamentDetailsRepository.GetAllAsync());

            return Ok(tournamentDetails);
        }

        #region old code
        // GET: api/TournamentDetails
        // This method retrieves all TournamentDetails records from the database.
        // It returns a 200 OK response with the collection of entities.
        // Data access is done via the Unit of Work abstraction (uoW).
        //public async Task<ActionResult<IEnumerable<TournamentDetails>>> GetTournamentDetails()
        //{
        //    // Retrieve all TournamentDetails records via the repository
        //    IEnumerable<TournamentDetails> tournamentDetails = await uoW.TournamentDetailsRepository.GetAllAsync();
        //    // Alternatively, you could use eager loading for related entities:
        //    // return await context.TournamentDetails.Include(g => g.Games).ToListAsync();

        //    // Return the results with HTTP 200 OK
        //    return Ok(tournamentDetails);
        //}
        #endregion

        /// <summary>
        /// Retrieves a single TournamentDetails record by its ID.
        /// </summary>
        /// <param name="id">The ID of the tournament to retrieve.</param>
        /// <param name="includeGames">Optional. Whether to include related games in the result.</param>
        /// <returns>
        /// Returns a 200 OK response with the tournament data if found; 
        /// otherwise, returns 404 Not Found.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TournamentDto>> GetTournamentDetails(int id, [FromQuery] bool includeGames = false)
        {
            // Attempt to find the entity using the Unit of Work pattern
            TournamentDetails? tournamentEntity = await uoW.TournamentDetailsRepository.GetAsync(id, includeGames);

            // Return 404 Not Found if the entity doesn't exist
            if(tournamentEntity == null) {
                return NotFound($"Tournament with ID {id} was not found.");
            }

            //Map to the Dto.
            TournamentDto tournamentDto = mapper.Map<TournamentDto>(tournamentEntity);

            // Return the found entity with HTTP 200 OK + JSON by default.
            // ASP.NET Core automatically wraps it as Ok(tournamentDetails)
            return Ok(tournamentDto);
        }

        #endregion

        #region PUT api/Games/5

        /// <summary>
        /// Updates an existing TournamentDetails record by its ID.
        /// </summary>
        /// <param name="id">The ID of the tournament to update. Must match an existing record.</param>
        /// <param name="tournamentUpdateDto">The data transfer object containing updated tournament fields (e.g., Title, StartDate).</param>
        /// <returns>
        /// Returns 204 No Content if the update is successful.
        /// Returns 400 Bad Request if the model is invalid.
        /// Returns 404 Not Found if the tournament with the specified ID does not exist.
        /// </returns>
        /// <remarks>
        /// This method uses the Unit of Work pattern to update the entity.
        /// It validates the incoming model using data annotations and applies changes only for the fields provided in the DTO.
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTournamentDetails(int id, [FromBody] TournamentUpdateDto tournamentUpdateDto)
        {
            // Validate the model state, checks data annotations.
            if(!ModelState.IsValid) {
                // Return 400 Bad Request with validation errors
                return BadRequest(ModelState);
            }

            // Retrieve the existing entity by ID
            TournamentDetails? existingTournamentDetails = await uoW.TournamentDetailsRepository.GetAsync(id);

            // Ensure the route ID matches the payload ID
            //if(id != tournamentDetails.Id) {
            if(existingTournamentDetails == null || existingTournamentDetails.Id != id) {
                // 400 Bad Request if IDs don't match
                //return BadRequest();
                //return BadRequest($"ID in the route ({id}) does not match ID in the payload ({tournamentUpdateDto.Id}).");
                return NotFound($"Tournament with ID {id} was not found.");
            }

            // Map the update DTO to the existing entity.
            // This will update only the fields specified in the DTO.
            mapper.Map(tournamentUpdateDto, existingTournamentDetails);

            // Mark the entity as modified in the Unit of Work pattern
            //uoW.TournamentDetailsRepository.Update(tournamentDetails);
            // Alternatively, for direct context access:
            // context.Entry(tournamentDetails).State = EntityState.Modified;

            try {
                // Attempt to save changes to the database
                uoW.TournamentDetailsRepository.Update(existingTournamentDetails);
                // Alternatively, for direct context access:
                //await context.SaveChangesAsync();

                // Persist the changes to the database
                await uoW.CompleteAsync();
            } catch(DbUpdateConcurrencyException) {

                // If the entity no longer exists, return 404 Not Found
                bool exists = await TournamentDetailsExists(existingTournamentDetails);
                if(!exists) {
                    return NotFound($"TournamentDetails with ID {existingTournamentDetails.Id} was not found. It may have been deleted or does not exist.");
                } else {
                    // Otherwise re-throw the exception to bubble up
                    throw;
                }
            }

            // The update was successful; return HTTP 204 No Content as per REST convention
            return NoContent();
        }

        #endregion

        #region POST api/TournamentDetails

        /// <summary>
        /// Creates a new <see cref="TournamentDetails"/> entity. POST: api/TournamentDetails
        /// </summary>
        /// <param name="tournamentDetailsCreateDto">The data transfer object containing details to create a new tournament.</param>
        /// <returns>
        /// Returns a 201 Created response with the created tournament data if successful.
        /// Returns 400 Bad Request if the model state is invalid or the input is null.
        /// Returns 409 Conflict if a tournament with the same title and start date already exists.
        /// </returns>
        /// <remarks>
        /// This method validates the input, checks for duplicates to prevent conflicts,
        /// maps the DTO to the entity, adds it to the repository, and saves changes asynchronously.
        /// It follows RESTful conventions by returning the location of the newly created resource in the response.
        /// </remarks>
        [HttpPost]
        // public async Task<ActionResult<TournamentDetails>> PostTournamentDetails(TournamentDetails tournamentDetails)
        public async Task<ActionResult<TournamentDetails>> PostTournamentDetails(TournamentDetailsCreateDto tournamentDetailsCreateDto)
        {
            // Validate the model stat, checks data annotations.
            if(!ModelState.IsValid) {
                // Return 400 Bad Request with validation errors
                return BadRequest(ModelState);
            }

            // Check for null input.
            if(tournamentDetailsCreateDto == null) {
                return BadRequest("TournamentDetails cannot be null.");
            }


            // Optional: check for duplicate tournament by Id.
            bool exists = await TournamentDetailsExists(tournamentDetailsCreateDto.Title,tournamentDetailsCreateDto.StartDate);

            if(exists) {
                return Conflict($"A tournament with the same name \"{tournamentDetailsCreateDto.Title}\" and start date already exists.");
            }

            // Map the DTO to the TournamentDetails entity.
            TournamentDetails tournamentDetails = mapper.Map<TournamentDetails>(tournamentDetailsCreateDto);

            // Add the new TournamentDetails entity to the repository
            uoW.TournamentDetailsRepository.Add(tournamentDetails);
            // Alternatively, using direct DbContext access:
            //context.TournamentDetails.Add(tournamentDetails);

            // Persist the changes to the database
            await uoW.CompleteAsync();
            // Alternatively, using direct DbContext access:
            //await context.SaveChangesAsync();

            // Map the created entity back to a DTO for the response
            // this is useful if you want to return a simplified view of the created resource.
            var tournamentDto = mapper.Map<TournamentDto>(tournamentDetails);
            // Return 201 Created with the route to access the new resource.
            // This follows REST conventions by providing a location header pointing to the new resource.
            return CreatedAtAction(nameof(GetTournamentDetails), new { id = tournamentDetails.Id }, tournamentDto);
        }

        #endregion

        #region DELETE api/TournamentDetails/5

        // DELETE: api/TournamentDetails/5
        // This method deletes an existing TournamentDetails entity by ID.
        // It follows REST conventions by returning 204 No Content if the deletion is successful.
        // If the specified entity does not exist, it returns 404 Not Found.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTournamentDetails(int id)
        {
            // Attempt to retrieve the entity by ID
            TournamentDetails? tournamentDetails = await uoW.TournamentDetailsRepository.GetAsync(id);

            // Alternatively, using direct DbContext access:
            //var tournamentDetails = await context.TournamentDetails.FindAsync(id);

            // If the entity does not exist, return 404 Not Found
            if(tournamentDetails == null) {
                return NotFound("Tournament with the specified ID was not found.");
            }

            // Remove the entity from the repository
            uoW.TournamentDetailsRepository.Remove(tournamentDetails);
            // Alternatively, using direct DbContext access:
            //context.TournamentDetails.Remove(tournamentDetails);

            // Persist the change to the database
            await uoW.CompleteAsync();

            // Alternatively, using direct DbContext access:
            //await context.SaveChangesAsync();

            // Return 204 No Content to indicate successful deletion
            return NoContent();
        }

        #endregion

        // This method checks whether a TournamentDetails entity with the specified ID exists in the database.
        // It is used to verify existence before update or delete operations to avoid conflicts or exceptions.
        private async Task<bool> TournamentDetailsExists(TournamentDetails tournamentDetails)
        {
            // Check for existence using the Game ID
            // Returns true if a Game with the same ID exists, otherwise false
            return await uoW.TournamentDetailsRepository.AnyAsync(tournamentDetails.Id);

            // Alternatively, using direct DbContext access:
            //return context.TournamentDetails.Any(e => e.Id == id);
        }

        private async Task<bool> TournamentDetailsExists(string title, DateTime startDate)
        {
            return await uoW.TournamentDetailsRepository
                .ExistsByTitleAndStartDateAsync(title, startDate);
        }

    }
}
