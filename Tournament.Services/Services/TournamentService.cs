using AutoMapper;
using Domain.Contracts;
using Service.Contracts;
using Tournaments.Shared.Dto;

namespace Tournaments.Services.Services;
public class TournamentService(IMapper mapper, IUnitOfWork uoW) : ITournamentService
{
    #region Get

    public async Task<IEnumerable<TournamentDto>> GetAllAsync(bool includeGames, bool trackChanges = false)
    {
        return mapper.Map<IEnumerable<TournamentDto>>(await uoW
            .TournamentDetailsRepository
            .GetAllAsync(includeGames, trackChanges));
    }

    public async Task<TournamentDto> GetByIdAsync(int id, bool trackChanges = false)
    {
        return mapper.Map<TournamentDto>(await uoW
            .TournamentDetailsRepository
            .GetAsync(id, trackChanges));
    }

    #endregion

}
