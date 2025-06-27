using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface IGameRepository
{

    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> GetAsync(int gameId);
    Task<bool> AnyAsync(int id);

    Task<bool> ExistsByNameAndDateAsync(string name, DateTime date);
    Task<Game?> GetByNameAndDateAsync(string name, int tournamentId);

    Task<IEnumerable<Game?>> GetByTournamentIdAsync(int tournamentId);

    void Add(Game game);
    void Update(Game game);
    void Remove(Game game);
}
