using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Service.Contracts;
using Tournaments.Shared.Dto;

namespace Tournaments.Services.Services;
public class TournamentService(IMapper mapper, IUnitOfWork uoW) : ITournamentService
{
    #region Get Tournament details

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

    #region PUT Tournament details

    #endregion

    #region POST Tournament details

    public async Task<(int id, TournamentDto tournamentDto)> CreateAsync(TournamentDetailsCreateDto tournamentDetailsCreateDto)
    {
        // Map the DTO to the TournamentDetails entity.
        TournamentDetails tournamentDetails = mapper.Map<TournamentDetails>(tournamentDetailsCreateDto);
        // Add the new TournamentDetails entity to the repository
        uoW.TournamentDetailsRepository.Add(tournamentDetails);
        // Persist the changes to the database
        await uoW.CompleteAsync();
        // Return the ID of the newly created tournament and the mapped DTO as a tuple.
        return (tournamentDetails.Id, mapper.Map<TournamentDto>(tournamentDetails));

    }

    #endregion

    public async Task<bool> ExistsAsync(string title, DateTime startDate)
    {
        return await uoW.TournamentDetailsRepository
            .ExistsByTitleAndStartDateAsync(title, startDate);
    }
}
