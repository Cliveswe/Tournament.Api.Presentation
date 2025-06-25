// Ignore Spelling: Api

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Api.Controllers
{
    [Route("api/tournamentDetails")]
    [ApiController]
    public class TournamentDetailsController(TournamentApiContext context, IUoW uoW) : ControllerBase
    {
        #region GET api/TournamentDetails api/TournamentDetails/5

        // GET: api/TournamentDetails
        // This method retrieves all TournamentDetails records from the database.
        // It returns a 200 OK response with the collection of entities.
        // Data access is done via the Unit of Work abstraction (uoW).
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TournamentDetails>>> GetTournamentDetails()
        {
            // Retrieve all TournamentDetails records via the repository
            IEnumerable<TournamentDetails> tournamentDetails = await uoW.TournamentDetailsRepository.GetAllAsync();
            // Alternatively, you could use eager loading for related entities:
            // return await context.TournamentDetails.Include(g => g.Games).ToListAsync();

            // Return the results with HTTP 200 OK
            return Ok(tournamentDetails);
        }

        // GET: api/TournamentDetails/5
        // This method retrieves a single TournamentDetails entity by its ID.
        // If the entity is not found, it returns a 404 Not Found response.
        // If found, the entity is returned with a 200 OK response.
        [HttpGet("{id}")]
        public async Task<ActionResult<TournamentDetails>> GetTournamentDetails(int id)
        {
            // Attempt to find the entity using the Unit of Work pattern
            TournamentDetails? tournamentDetails = await uoW.TournamentDetailsRepository.GetAsync(id);

            // Return 404 Not Found if the entity doesn't exist
            if(tournamentDetails == null) {
                return NotFound();
            }

            // Return the found entity with HTTP 200 OK
            // ASP.NET Core automatically wraps it as Ok(tournamentDetails)
            return tournamentDetails;// Returns 200 OK + JSON by default
        }

        #endregion

        #region PUT api/Games/5

        // PUT: api/TournamentDetails/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // This method updates an existing TournamentDetails entity.
        // It follows REST conventions by returning 204 NoContent if the update is successful.
        // To protect against over-posting attacks, bind only the fields you want to update.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTournamentDetails(int id, TournamentDetails tournamentDetails)
        {
            // Ensure the route ID matches the payload ID
            if(id != tournamentDetails.Id) {
                // 400 Bad Request if IDs don't match
                return BadRequest();
            }

            // Mark the entity as modified in the Unit of Work pattern
            //uoW.TournamentDetailsRepository.Update(tournamentDetails);
            // Alternatively, for direct context access:
            // context.Entry(tournamentDetails).State = EntityState.Modified;

            try {
                // Attempt to save changes to the database
                uoW.TournamentDetailsRepository.Update(tournamentDetails);
                // Alternatively, for direct context access:
                //await context.SaveChangesAsync();

                // Persist the changes to the database
                await uoW.CompleteAsync();
            } catch(DbUpdateConcurrencyException) {

                // If the entity no longer exists, return 404 Not Found
                bool exists = await TournamentDetailsExists(tournamentDetails);
                if(!exists) {
                    return NotFound($"TournamentDetails with ID {tournamentDetails.Id} was not found. It may have been deleted or does not exist.");
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

        // POST: api/TournamentDetails
        // This method creates a new TournamentDetails entity.
        // It follows REST conventions by returning 201 Created with a location header pointing to the new resource.
        // To protect against over-posting attacks, bind only the fields you want to allow clients to set.
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TournamentDetails>> PostTournamentDetails(TournamentDetails tournamentDetails)
        {
            // Validate the model stat, checks data annotations.
            if(!ModelState.IsValid) {
                // Return 400 Bad Request with validation errors
                return BadRequest(ModelState);
            }

            // Check for null input.
            if(tournamentDetails == null) {
                return BadRequest("TournamentDetails cannot be null.");
            }

            // Optional: check for duplicate tournament by Id.
            bool exists = await TournamentDetailsExists(tournamentDetails);

            if(exists) {
                return Conflict("A tournament with the same name and start date already exists.");
            }

            // Add the new TournamentDetails entity to the repository
            uoW.TournamentDetailsRepository.Add(tournamentDetails);
            // Alternatively, using direct DbContext access:
            //context.TournamentDetails.Add(tournamentDetails);

            // Persist the changes to the database
            await uoW.CompleteAsync();
            // Alternatively, using direct DbContext access:
            //await context.SaveChangesAsync();

            // Return 201 Created with the route to access the new resource
            return CreatedAtAction("GetTournamentDetails", new { id = tournamentDetails.Id }, tournamentDetails);
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
    }
}
