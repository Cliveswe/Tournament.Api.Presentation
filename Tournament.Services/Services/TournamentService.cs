using AutoMapper;
using Domain.Contracts;
using Service.Contracts;
using Tournament.Shared.Dto;

namespace Tournament.Services.Services;
public class TournamentService(IMapper mapper, IUoW uoW) : ITournamentService
{
    public async Task<IEnumerable<TournamentDto>> GetTournamentsAsync(bool includeGames, bool trackChanges = false)
    {
        return mapper.Map<IEnumerable<TournamentDto>>(await uoW.
            TournamentDetailsRepository.GetAllAsync(includeGames, trackChanges));
    }

    public Task<TournamentDto> TournamentAsync(int id, bool trackChanges = false)
    {
        throw new NotImplementedException();
    }
}
