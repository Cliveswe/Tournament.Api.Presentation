// Ignore Spelling: Api

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Api.Controllers
{
    [Route("api/games")]
    [ApiController]
    public class GamesController(TournamentApiContext context, IUoW uoW) : ControllerBase
    {

        // GET: api/Games
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetGame()
        {
            var games = await uoW.GameRepository.GetAllAsync();
            //return await context.Game.ToListAsync();
            return Ok(games); // Wrap the result in Ok()
        }

        // GET: api/Games/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            // var game = await context.Game.FindAsync(id);
            var game = await uoW.GameRepository.GetAsync(id);

            if(game == null) {
                return NotFound();
            }

            return game;
        }

        // PUT: api/Games/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGame(int id, Game game)
        {
            if(id != game.Id) {
                return BadRequest();
            }

            uoW.GameRepository.Update(game);
            //   context.Entry(game).State = EntityState.Modified;

            try {
                await context.SaveChangesAsync();

            } catch(DbUpdateConcurrencyException) {
                var exists = await GameExists(id);
                if(!exists) {
                    return NotFound();
                } else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Games
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Game>> PostGame(Game game)
        {
            //context.Game.Add(game);
            uoW.GameRepository.Add(game);

            await context.SaveChangesAsync();

            return CreatedAtAction("GetGame", new { id = game.Id }, game);
        }

        // DELETE: api/Games/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            // var game = await context.Game.FindAsync(id);
            var game = await uoW.GameRepository.GetAsync(id);

            if(game == null) {
                return NotFound();
            }

            //context.Game.Remove(game);
            uoW.GameRepository.Remove(game);

            //await context.SaveChangesAsync();
            await uoW.CompleteAsync();

            return NoContent();
        }

        private async Task<bool> GameExists(int id)
        {
            //return context.Game.Any(e => e.Id == id);
            return await uoW.GameRepository.AnyAsync(id);

        }
    }
}
