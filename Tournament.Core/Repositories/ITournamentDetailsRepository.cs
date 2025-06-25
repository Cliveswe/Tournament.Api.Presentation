using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface ITournamentDetailsRepository
{
    Task<IEnumerable<TournamentDetails>> GetAllAsync();
    Task<TournamentDetails?> GetAsync(int tournamentId);
    Task<bool> AnyAsync(int id);
    void Add(TournamentDetails tournament);
    void Update(TournamentDetails tournament);
    void Remove(TournamentDetails tournament);
}
