using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class GameRepository(TournamentApiContext context) : ITournamentRepository
{
    public void Add(TournamentDetails tournament)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AnyAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TournamentDetails>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<TournamentDetails> GetAsync(int tournamentId)
    {
        throw new NotImplementedException();
    }

    public void Remove(TournamentDetails tournament)
    {
        throw new NotImplementedException();
    }

    public void Update(TournamentDetails tournament)
    {
        throw new NotImplementedException();
    }
}
