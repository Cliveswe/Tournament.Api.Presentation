// Ignore Spelling: Dto

using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using Domain.Models.Responses;
using Service.Contracts;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Tournaments.Services.Services;
public class TournamentService(IMapper mapper, IUnitOfWork uoW) : ITournamentService
{
    #region Get Tournament details

    //public async Task<IEnumerable<TournamentDto>> GetAllAsync(TournamentRequestParameters requestParameters, bool trackChanges = false)
    public async Task<(ApiBaseResponse tournamentDto, MetaData metaData)> GetAllAsync(TournamentRequestParameters requestParameters, bool trackChanges = false)
    {
        TournamentRequestParameters clampedParameters =  ServiceManager.ClampRequestParameters(requestParameters);

        var pagedList = await uoW
            .TournamentDetailsRepository
            .GetAllAsync(requestParameters, trackChanges);

        // Clamp PageNumber if it exceeds total pages
        if(pagedList.MetaData.TotalPages > 0 && clampedParameters.PageNumber > pagedList.MetaData.TotalPages) {

            clampedParameters.PageNumber = pagedList.MetaData.TotalPages;

            // Fetch again with corrected PageNumber
            pagedList = await uoW
           .TournamentDetailsRepository
           .GetAllAsync(requestParameters, trackChanges);
        }

        var tournamentDtos = mapper.Map<IEnumerable<TournamentDto>>(pagedList.Items);

        //return (tournamentDtos, pagedList.MetaData);
        return tournamentDtos.Any() ? (new ApiOkResponse<IEnumerable<TournamentDto>>(tournamentDtos), pagedList.MetaData) : (new TournamentNotFoundResponse("No tournaments found."), pagedList.MetaData);
    }

    public async Task<TournamentDto> GetByIdAsync(int id, bool trackChanges = false)
    {
        var tournamentDetails = await uoW
            .TournamentDetailsRepository
            .GetAsync(id, trackChanges);

        if(tournamentDetails is null) {
            throw new TournamentNotFoundException(id);
        }

        return mapper.Map<TournamentDto>(tournamentDetails);
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

    public async Task<ApiBaseResponse> ExistsAsync(string title, DateTime startDate)
    {
        bool entityExists = await uoW.TournamentDetailsRepository
            .ExistsByTitleAndStartDateAsync(title, startDate);
        return entityExists ? new ApiOkResponse<bool>(entityExists) : new ApiNotFoundResponse($"Tournament {title} with start date {startDate} does not exists.");
    }

    public async Task<ApiBaseResponse> ExistsAsync(int id)
    {
        bool entityExists = await uoW.TournamentDetailsRepository.AnyAsync(id);
        return entityExists ? new ApiOkResponse<bool>(entityExists) : new ApiNotFoundResponse("Tournament does not exists.");
    }

    #endregion
}
