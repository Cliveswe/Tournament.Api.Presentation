// Ignore Spelling: Api Dto Ok

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Dto;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;

namespace Tournament.Api.Controllers
{

    [ApiController]
    [Route("api/tournamentDetails/{tournamentId}/games")]
    //public class GamesController(TournamentApiContext context) : ControllerBase
    public class GamesController(IMapper mapper, IUoW uoW) : ControllerBase
    {
        #region GET api/Games api/1/Games/

        /// <summary>
        /// Retrieves all Game entities associated with a specific TournamentDetails ID.
        /// [HttpGet("/api/tournamentDetails/{tournametId}/games")]
        /// </summary>
        /// <param name="tournamentId">The ID of the tournament for which to fetch games.</param>
        /// <returns>
        /// Returns a 200 OK response containing a collection of GameDto objects if the tournament exists.
        /// Returns 404 Not Found if the tournament with the specified ID does not exist.
        /// </returns>
        /// Note that the route is overridden and it is important to add a root "/api/..." and 
        /// not "api/..." there is a difference.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetTournamentGame(int tournamentId)
        {
            // Validate the tournament ID from the route parameter
            bool exists = await uoW.TournamentDetailsRepository.AnyAsync(tournamentId);
            // If the tournament with the specified ID does not exist, return 404 Not Found
            if(!exists) {
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            // Fetch all Game records using the GameRepository
            //IEnumerable<Game> games = await uoW.GameRepository.GetAllAsync();

            // Check if any games exist for the specified tournament ID
            //IEnumerable<Game> gamesExist = await uoW.GameRepository.GetAllAsync();
            //List<Game> gamesResult = gamesExist.Where(g => g.TournamentDetailsId == tournamentId).ToList();

            IEnumerable<Game?> gamesResult = await uoW.GameRepository.GetByTournamentIdAsync(tournamentId);



            IEnumerable<GameDto> games = mapper.Map<IEnumerable<GameDto>>(gamesResult);
            // Alternatively, direct EF Core access could be used:
            //return await context.Game.ToListAsync();

            // Return the result with HTTP 200 OK
            return Ok(games);
        }

        /// <summary>
        /// Retrieves a single Game entity by its ID. GET: api/Games/5
        /// </summary>
        /// <param name="id">The ID of the gameEntity to retrieve.</param>
        /// <returns>
        /// Returns a 200 OK response with the gameEntity data if found;
        /// otherwise, returns 404 Not Found.
        /// </returns>
        [HttpGet("/api/games/{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            // Attempt to retrieve the gameEntity using the GameRepository
            Game? game = await uoW.GameRepository.GetAsync(id);

            // Return 404 if the gameEntity does not exist
            if(game == null) {
                return NotFound($"Game with ID {id} was not found.");
            }
            GameDto gameDto = mapper.Map<GameDto>(game);
            // Return the gameEntity with HTTP 200 OK
            // ASP.NET Core infers this as Ok(gameEntity) for ActionResult<T>
            return Ok(gameDto);// Returns 200 OK + JSON by default
        }

        #endregion

        #region PUT api/Games/5

        /// <summary>
        /// Updates an existing gameEntity identified by its ID with the provided data. PUT: api/Games/5
        /// </summary>
        /// <param name="id">The ID of the gameEntity to update.</param>
        /// <param name="gameUpdateDto">The updated gameEntity data provided in the request body.</param>
        /// <returns>
        /// Returns a <see cref="BadRequestResult"/> if the model state is invalid or the ID doesn't match.
        /// Returns <see cref="NotFoundResult"/> if the gameEntity does not exist.
        /// Returns <see cref="NoContentResult"/> if the update is successful.
        /// </returns>
        /// <remarks>
        /// This endpoint follows REST conventions for PUT operations. It uses AutoMapper to apply changes from the DTO to the entity.
        /// The GameUpdateDto includes only the fields allowed to be updated (e.g., Title).
        /// A 204 No Content response indicates that the update succeeded without returning a body.
        /// </remarks>
        [HttpPut("/api/games/{id}")]
        //public async Task<IActionResult> PutGame(int id, Game gameEntity)
        public async Task<IActionResult> PutGame(int id, [FromBody] GameUpdateDto gameUpdateDto)
        {
            if(!ModelState.IsValid) {
                // Return 400 Bad Request if the model state is invalid
                return BadRequest(ModelState);
            }

            Game? existingGame = await uoW.GameRepository.GetAsync(id);

            // Ensure the route ID matches the payload ID
            //if(id != gameEntity.Id) {
            if(existingGame == null || existingGame.Id != id) {
                // 400 Bad Request if IDs don't match
                return BadRequest();
            }

            // Mark the entity as modified in the Unit of Work pattern
            // uoW.GameRepository.Update(gameEntity);
            // Alternatively, for direct context access:
            // context.Entry(gameEntity).State = EntityState.Modified;

            // Map the DTO to the existing entity.
            // This will update only the fields specified in the DTO.
            mapper.Map(gameUpdateDto, existingGame);

            try {
                // Attempt to save changes to the database
                uoW.GameRepository.Update(existingGame);

                // Persist the changes to the database
                await uoW.CompleteAsync();

                // Alternatively, for direct context access:
                //await context.SaveChangesAsync();

            } catch(DbUpdateConcurrencyException) {

                bool exists = await GameExists(existingGame);
                // If the entity no longer exists, return 404 Not Found
                if(!exists) {
                    return NotFound($"Game with ID {existingGame.Id} was not found. It may have been deleted or does not exist.");
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
        public async Task<ActionResult<GameDto>> PostGame([FromBody] GameCreateDto gameCreateDto, [FromRoute] int tournamentId)
        {
            if(gameCreateDto == null) {
                // Return 400 Bad Request if the request body is null
                return BadRequest("Game data must be provided.");
            }

            // Validate the model state using data annotations.
            if(!ModelState.IsValid) {
                // Return 400 Bad Request with validation errors
                return BadRequest(ModelState);
            }

            if(gameCreateDto.TournamentDetailsId != tournamentId) {
                return BadRequest("Mismatch between route tournamentId and request body.");
            }

            // Trim whitespace from the gameEntity name
            gameCreateDto.Name = gameCreateDto.Name.Trim();
            if(string.IsNullOrWhiteSpace(gameCreateDto.Name)) {
                return BadRequest("Game name is required.");
            }

            // Map the DTO to the TournamentDto
            TournamentDetails? existingTournament = await uoW.TournamentDetailsRepository.GetAsync(tournamentId);
            // Validate tournament existence
            if(existingTournament == null) {
                // Return 404 Not Found if the tournament does not exist
                return NotFound($"Tournament with ID {tournamentId} does not exist.");
            }

            // Validate that the gameEntity time is within the tournament's start and end dates
            //if(gameCreateDto.Time <= tournamentDetailsDto.StartDate ||
            //    gameCreateDto.Time >= tournamentDetailsDto.EndDate) {
            //    return BadRequest($"Game time must be within the tournament's start \"{tournamentDetailsDto.StartDate}\" and end \"{tournamentDetailsDto.EndDate}\" dates.");
            //}

            TournamentDto tournamentDetailsDto = mapper.Map<TournamentDto>(existingTournament);
            if(!IsGameTimeValid(gameCreateDto.Time, tournamentDetailsDto)) {
                return BadRequest($"Game time must be within the tournament's start \"{tournamentDetailsDto.StartDate}\" and end \"{tournamentDetailsDto.EndDate}\" dates.");
            }

            // Optional: check if a similar gameEntity already exists.
            Game duplicateGame = await FindDuplicateGameAsync(gameCreateDto, tournamentId);

            // Use the new method to check for duplicate by name & date

            // bool exists = await uoW.GameRepository.ExistsByNameAndDateAsync(gameEntity.Title, gameEntity.Time);

            if(duplicateGame != null) {
                // Return 409 Conflict if a duplicate gameEntity is found
                //return Conflict($"A gameEntity with the same name \"{gameCreateDto.Name}\" and date already exists.");
                return Conflict($"A gameEntity named \"{gameCreateDto.Name}\" already exists on {gameCreateDto.Time:yyyy-MM-dd HH:mm}.");

            }

            // Add the new Game entity to the repository
            Game gameEntity = mapper.Map<Game>(gameCreateDto); // map DTO to Game entity
            uoW.GameRepository.Add(gameEntity);
            // Alternatively, using direct DbContext access:
            // context.Game.Add(gameEntity);

            try {
                // Persist the changes to the database
                var changes  = await uoW.CompleteAsync();
                if(changes == 0) {
                    // No changes saved - something went wrong
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save the new gameEntity.");
                }
            } catch(DbUpdateException) {
                // Log exception if you have logging setup
                return StatusCode(StatusCodes.Status500InternalServerError, "Database update error occurred.");
            }
            // Alternatively, using direct DbContext access:
            //await context.SaveChangesAsync();

            // Return 201 Created with the route to access the new resource
            return CreatedAtAction(nameof(GetGame), new { id = gameEntity.Id }, mapper.Map<GameDto>(gameEntity));
        }

        private async Task<Game> FindDuplicateGameAsync(GameCreateDto gameCreateDto, int tournamentId)
        {

            // Check if a gameEntity with the same name and time already exists in the specified tournament
            return await uoW.GameRepository.GetByNameAndDateAsync(gameCreateDto.Name, tournamentId);
        }

        private bool IsGameTimeValid(DateTime gameTime, TournamentDto tournament)
        {
            return gameTime > tournament.StartDate && gameTime < tournament.EndDate;
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
            // var gameEntity = await context.Game.FindAsync(id);

            // If the entity does not exist, return 404 Not Found
            if(game == null) {
                return NotFound("Game with the specified ID was not found.");
            }

            // Remove the Game entity from the repository
            uoW.GameRepository.Remove(game);
            // Alternatively, using direct DbContext access:
            // context.Game.Remove(gameEntity);

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
            // return context.Game.Any(e => e.Id == gameEntity.Id);

        }
    }
}
