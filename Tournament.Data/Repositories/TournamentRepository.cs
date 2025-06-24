using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class TournamentRepository(TournamentApiContext context) : ITournamentRepository
{
    public void Add(TournamentDetails tournament)
    {
        context.TournamentDetails.Add(tournament);
    }

    public Task<bool> AnyAsync(int id)
    {
        return context.TournamentDetails.AnyAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TournamentDetails>> GetAllAsync()
    {
        return await context.TournamentDetails
            .Include(g => g.Games)
            .ToListAsync();
    }

    public async Task<TournamentDetails?> GetAsync(int tournamentId)
    {
        return await context.TournamentDetails.FindAsync(tournamentId);
    }

    public void Remove(TournamentDetails tournament)
    {
        context.TournamentDetails.Remove(tournament);
    }

    public void Update(TournamentDetails tournament)
    {
        context.TournamentDetails.Update(tournament);
    }
}
