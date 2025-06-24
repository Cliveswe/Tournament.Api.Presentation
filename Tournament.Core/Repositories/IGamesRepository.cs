using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> GetAsync(int gameId);
    Task<bool> AnyAsync(int id);
    void Add(Game game);
    void Update(Game game);
    void Remove(Game game);
}
