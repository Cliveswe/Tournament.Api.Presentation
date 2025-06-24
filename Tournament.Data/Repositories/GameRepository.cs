using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class GameRepository(TournamentApiContext context) : IGameRepository
{
    public void Add(Game game)
    {
        context.Game.Add(game);
    }

    public async Task<bool> AnyAsync(int id)
    {
        return await context.Game.AnyAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await context.Game.ToListAsync();
    }

    public async Task<Game?> GetAsync(int gameId)
    {
        return await context.Game.FindAsync(gameId);
    }

    public void Remove(Game game)
    {
        context.Game.Remove(game);
    }

    public void Update(Game game)
    {
        context.Game.Update(game);
    }
}
