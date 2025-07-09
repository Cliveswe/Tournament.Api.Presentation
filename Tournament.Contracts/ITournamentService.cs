using Tournaments.Shared.Dto;

namespace Service.Contracts;

public interface ITournamentService
{

    Task<IEnumerable<TournamentDto>> GetTournamentsAsync(bool includeGames, bool trackChanges = false);
    Task<TournamentDto> GetTournamentByIdAsync(int id, bool trackChanges = false);

}