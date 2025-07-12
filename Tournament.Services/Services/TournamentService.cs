// Ignore Spelling: Dto

using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Service.Contracts;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Tournaments.Services.Services;
public class TournamentService(IMapper mapper, IUnitOfWork uoW) : ITournamentService
{
    #region Get Tournament details

    //public async Task<IEnumerable<TournamentDto>> GetAllAsync(TournamentRequestParameters requestParameters, bool trackChanges = false)
    public async Task<(IEnumerable<TournamentDto> tournamentDto, MetaData metaData)> GetAllAsync(TournamentRequestParameters requestParameters, bool trackChanges = false)
    {
        var pagedList = await uoW
            .TournamentDetailsRepository
            .GetAllAsync(requestParameters, trackChanges);

        var tournamentDtos = mapper.Map<IEnumerable<TournamentDto>>(pagedList.Items);

        //return mapper.Map<IEnumerable<TournamentDto>>(await uoW
        //    .TournamentDetailsRepository
        //    .GetAllAsync(requestParameters, trackChanges));

        return (tournamentDtos, pagedList.MetaData);
    }

    public async Task<TournamentDto> GetByIdAsync(int id, bool trackChanges = false)
    {
        return mapper.Map<TournamentDto>(await uoW
            .TournamentDetailsRepository
            .GetAsync(id, trackChanges));
    }

    #endregion

    #region PATCH Tournament details

    public async Task<bool> ApplyToAsync(int id, TournamentDto tournamentDto)
    {
        // Retrieve the existing tournament details.
        TournamentDetails? existingEntity = await uoW.TournamentDetailsRepository.GetAsync(id, trackChanges: true);
        if(existingEntity is null) {
            return false;
        }

        // Map the incoming DTO to the existing tournament details.
        mapper.Map(tournamentDto, existingEntity);

        // Update the existing tournament details in the repository.
        uoW.TournamentDetailsRepository.Update(existingEntity);
        // Persist the changes to the database.
        await uoW.CompleteAsync();

        return true;
    }

    #endregion

    #region PUT Tournament details

    public async Task<bool> Update(int id, TournamentUpdateDto tournamentUpdateDto)
    {
        // Attempt to retrieve the tournament details by ID.
        var tournamentDetails = await uoW.TournamentDetailsRepository.GetAsync(id);

        // If the tournament exists, map the DTO to the entity.
        if(tournamentDetails != null) {

            // Map the update DTO to the existing tournament details entity.
            mapper.Map(tournamentUpdateDto, tournamentDetails);

            // Update the tournament details in the repository.
            uoW.TournamentDetailsRepository.Update(tournamentDetails);

            // Persist the changes to the database.
            await uoW.CompleteAsync();
            return true;
        }

        return false;
    }

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

    #region DELETE Tournament details

    public async Task<bool> RemoveAsync(int id)
    {
        // Attempt to retrieve the tournament details by ID.
        var tournamentDetails = await uoW.TournamentDetailsRepository.GetAsync(id);

        // If the tournament exists, remove it from the repository.
        if(tournamentDetails != null) {
            // Remove the tournament details from the repository
            uoW.TournamentDetailsRepository.Remove(tournamentDetails);
            // Persist the change to the database
            await uoW.CompleteAsync();
            return true;
        }

        // If the tournament does not exist, return false.
        return false;
    }

    #endregion

    #region Exists Tournament details

    public async Task<bool> ExistsAsync(string title, DateTime startDate)
    {
        return await uoW.TournamentDetailsRepository
            .ExistsByTitleAndStartDateAsync(title, startDate);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await uoW.TournamentDetailsRepository.AnyAsync(id);
    }

    #endregion
}
