using Tournaments.Shared.Dto;

namespace Service.Contracts;

public interface ITournamentService
{

    Task<IEnumerable<TournamentDto>> GetAllAsync(bool includeGames, bool trackChanges = false);
    Task<TournamentDto> GetByIdAsync(int id, bool trackChanges = false);
    Task<(int id, TournamentDto tournamentDto)> CreateAsync(TournamentDetailsCreateDto tournamentDetailsCreateDto);

    Task<bool> Update(int id, TournamentUpdateDto tournamentUpdateDto);
    Task<bool> ExistsAsync(string title, DateTime startDate);
    Task<bool> ExistsAsync(int id);
    Task<bool> RemoveAsync(int id);
}