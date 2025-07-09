using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Service.Contracts;
using Tournaments.Shared.Dto;

namespace Tournaments.Services.Services;
public class TournamentService(IMapper mapper, IUnitOfWork uoW) : ITournamentService
{
    public async Task<IEnumerable<TournamentDto>> GetTournamentsAsync(bool includeGames, bool trackChanges = false)
    {
        return mapper.Map<IEnumerable<TournamentDto>>(await uoW
            .TournamentDetailsRepository
            .GetAllAsync(includeGames, trackChanges));
    }

    public async Task<TournamentDetails> GetTournamentByIdAsync(int id, bool trackChanges = false)
    {
        return mapper.Map<TournamentDetails>(await uoW
            .TournamentDetailsRepository
            .GetAsync(id, trackChanges));
    }
}
