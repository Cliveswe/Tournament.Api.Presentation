using Tournament.Shared.Dto;

namespace Service.Contracts;

public interface ITournamentService
{

    Task<IEnumerable<TournamentDto>> GetTournamentsAsync(bool includeGames, bool trackChanges = false);
    Task<TournamentDto> TournamentAsync(int id, bool trackChanges = false);

}