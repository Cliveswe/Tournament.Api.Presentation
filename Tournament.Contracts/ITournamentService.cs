using Domain.Models.Entities;
using Tournaments.Shared.Dto;

namespace Service.Contracts;

public interface ITournamentService
{

    Task<IEnumerable<TournamentDto>> GetTournamentsAsync(bool includeGames, bool trackChanges = false);
    Task<TournamentDetails> GetTournamentByIdAsync(int id, bool trackChanges = false);

}