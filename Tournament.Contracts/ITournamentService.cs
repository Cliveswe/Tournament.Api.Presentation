using Tournaments.Shared.Dto;

namespace Service.Contracts;

public interface ITournamentService
{

    Task<IEnumerable<TournamentDto>> GetAllAsync(bool includeGames, bool trackChanges = false);
    Task<TournamentDto> GetByIdAsync(int id, bool trackChanges = false);

}