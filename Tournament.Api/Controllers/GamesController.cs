// Ignore Spelling: Api

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Dto;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;

namespace Tournament.Api.Controllers
{
    [ApiController]
    [Route("api/games")]
    //public class GamesController(TournamentApiContext context) : ControllerBase
    public class GamesController(IMapper mapper, IUoW uoW) : ControllerBase
    {
        #region GET api/Games api/1/Games/

        // GET: api/Games
        // This method retrieves all Game entities from the database.
        // It returns a 200 OK response containing the full list of games.
        // Data is accessed through the Unit of Work abstraction.
        //Note that the route is overridden and it is important to add a root "/api/..." and 
        //not "api/..." there is a difference.
        [HttpGet("/api/tournamentDetails/{tournametId}/games")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetTournamentGame(int tournametId)
        {
            // Validate the tournament ID from the route parameter
            bool exists = await uoW.TournamentDetailsRepository.AnyAsync(tournametId);
            // If the tournament with the specified ID does not exist, return 404 Not Found
            if(!exists) {
                return NotFound($"Tournament with ID {tournametId} does not exist.");
            }

            // Fetch all Game records using the GameRepository
            //IEnumerable<Game> games = await uoW.GameRepository.GetAllAsync();

            // Check if any games exist for the specified tournament ID
            IEnumerable<Game> gamesExist = await uoW.GameRepository.GetAllAsync();
            List<Game> gamesResult = gamesExist.Where(g => g.TournamentDetailsId == tournametId).ToList();

            IEnumerable<GameDto> games = mapper.Map<IEnumerable<GameDto>>(gamesResult);
            // Alternatively, direct EF Core access could be used:
            //return await context.Game.ToListAsync();

            // Return the result with HTTP 200 OK
            return Ok(games);
        }

        // GET: api/Games/5
        // This method retrieves a single Game entity by its ID.
        // If the game is not found, it returns a 404 Not Found response.
        // Otherwise, it returns the game object with a 200 OK response.
        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            // Attempt to retrieve the game using the GameRepository
            Game? game = await uoW.GameRepository.GetAsync(id);

            // Return 404 if the game does not exist
            if(game == null) {
                return NotFound();
            }

            // Return the game with HTTP 200 OK
            // ASP.NET Core infers this as Ok(game) for ActionResult<T>
            return game;// Returns 200 OK + JSON by default
        }

        #endregion

        #region PUT api/Games/5

        // PUT: api/Games/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGame(int id, Game game)
        {
            // Ensure the route ID matches the payload ID
            if(id != game.Id) {
                // 400 Bad Request if IDs don't match
                return BadRequest();
            }

            // Mark the entity as modified in the Unit of Work pattern
            uoW.GameRepository.Update(game);
            // Alternatively, for direct context access:
            // context.Entry(game).State = EntityState.Modified;

            try {
                // Attempt to save changes to the database
                await uoW.CompleteAsync();
                // Alternatively, for direct context access:
                //await context.SaveChangesAsync();

            } catch(DbUpdateConcurrencyException) {

                bool exists = await GameExists(game);
                // If the entity no longer exists, return 404 Not Found
                if(!exists) {
                    return NotFound($"Game with ID {game.Id} was not found. It may have been deleted or does not exist.");
                } else {
                    // Otherwise re-throw the exception to bubble up
                    throw;
                }
            }


            // The update was successful; return HTTP 204 No Content as per REST convention
            return NoContent();
        }

        #endregion

        #region POST api/Games
        // POST: api/Games
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // This method creates a new Game entity.
        // It follows REST conventions by returning 201 Created with a location header pointing to the new resource.
        // To protect against over-posting attacks, bind only the fields you want to allow clients to set.
        [HttpPost]
        public async Task<ActionResult<Game>> PostGame(Game game)
        {

            // Validate the model state using data annotations.
            if(!ModelState.IsValid) {
                // Return 400 Bad Request with validation errors
                return BadRequest(ModelState);
            }

            // Check for null input.
            if(game == null) {
                return BadRequest("Game cannot be null.");
            }

            // Optional: check if a similar game already exists.
            //var exists = await GameExists(game);

            // Use the new method to check for duplicate by name & date
            bool exists = await uoW.GameRepository.ExistsByNameAndDateAsync(game.Title, game.Time);

            if(exists) {
                // Return 409 Conflict if a duplicate game is found
                return Conflict("A game with the same name and date already exists.");
            }

            // Add the new Game entity to the repository
            uoW.GameRepository.Add(game);
            // Alternatively, using direct DbContext access:
            // context.Game.Add(game);

            // Persist the changes to the database
            var changes  = await uoW.CompleteAsync();

            if(changes == 0) {
                // No changes saved - something went wrong
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save the new game.");
            }
            // Alternatively, using direct DbContext access:
            //await context.SaveChangesAsync();

            // Return 201 Created with the route to access the new resource
            return CreatedAtAction("GetGame", new { id = game.Id }, game);
        }

        #endregion

        #region DELETE api/Games/5

        // DELETE: api/Games/5
        // This method deletes an existing Game entity by ID.
        // It follows REST conventions by returning 204 No Content if the deletion is successful.
        // If the specified entity does not exist, it returns 404 Not Found.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            // Attempt to retrieve the Game entity by ID from the repository
            var game = await uoW.GameRepository.GetAsync(id);
            // Alternatively, using direct DbContext access:
            // var game = await context.Game.FindAsync(id);

            // If the entity does not exist, return 404 Not Found
            if(game == null) {
                return NotFound("Game with the specified ID was not found.");
            }

            // Remove the Game entity from the repository
            uoW.GameRepository.Remove(game);
            // Alternatively, using direct DbContext access:
            // context.Game.Remove(game);

            // Persist the change to the database
            await uoW.CompleteAsync();
            // Alternatively, using direct DbContext access:
            //await context.SaveChangesAsync();

            // Return 204 No Content to indicate successful deletion
            return NoContent();
        }

        #endregion

        // This method checks whether a Game entity with the specified ID exists in the database.
        // It is used to verify existence before update or delete operations to avoid conflicts or exceptions.
        private async Task<bool> GameExists(Game game)
        {
            // Check for existence using the Game ID
            // Returns true if a Game with the same ID exists, otherwise false
            return await uoW.GameRepository.AnyAsync(game.Id);

            // Alternatively, using direct DbContext access:
            // return context.Game.Any(e => e.Id == game.Id);

        }
    }
}
