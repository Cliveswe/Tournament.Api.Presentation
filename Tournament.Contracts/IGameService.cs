using Tournaments.Shared.Dto;

namespace Service.Contracts;

public interface IGameService
{
    Task<IEnumerable<GameDto>> GetAllAsync(int tournamentId);
    Task<GameDto> GetAsync(int tournamentId, int id);

    Task<GameDto> GetAsync(int tournamentId, string title);
    Task<bool> RemoveAsync(int tournamentId, int id);
    Task<bool> ExistsAsync(int id);
}