using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface ITournamentDetailsRepository
{
    //    Task<IEnumerable<TournamentDetails>> GetAllAsync();
    Task<IEnumerable<TournamentDetails>> GetAllAsync(bool includeGames = false);
    Task<TournamentDetails?> GetAsync(int tournamentId, bool includeGAmes = false);
    Task<bool> AnyAsync(int id);
    void Add(TournamentDetails tournament);
    void Update(TournamentDetails tournament);
    void Remove(TournamentDetails tournament);
}
