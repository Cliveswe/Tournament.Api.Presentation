// Ignore Spelling: Dto

using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
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

    public async Task<ApiBaseResponse> GetByIdAsync(int id, bool trackChanges = false)
    {
        TournamentDetails? tournamentDetails = await uoW
            .TournamentDetailsRepository
            .GetAsync(id, trackChanges);

        if(tournamentDetails is null || tournamentDetails.Id != id) {
            return new TournamentNotFoundResponse($"Tournament with ID {id} was not found.");
        }

        return new ApiOkResponse<TournamentDto>(mapper.Map<TournamentDto>(tournamentDetails));
    }

    #endregion

    #region PATCH Tournament details

    public async Task<ApiBaseResponse> ApplyToAsync(int id, TournamentDto tournamentUpdateDto)
    {
        // Retrieve the existing tournament details.
        TournamentDetails? existingEntity = await uoW.TournamentDetailsRepository.GetAsync(id, trackChanges: true);
        if(existingEntity is null) {
            return new TournamentNotFoundResponse($"Tournament with id {id} was not found.");
        }

        // Map the incoming DTO to the existing tournament details.
        mapper.Map(tournamentUpdateDto, existingEntity);

        // UpdateAsync the existing tournament details in the repository.
        uoW.TournamentDetailsRepository.Update(existingEntity);
        // Persist the changes to the database.
        int result = await uoW.CompleteAsync();

        if(result == 0)
            return new NotModifiedResponse("The tournament was not updated.");

        // Fetch fresh data from DB to reflect all updates
        TournamentDetails? updatedEntity = await uoW.TournamentDetailsRepository.GetAsync(id, trackChanges: false);
        TournamentDto updatedDto = mapper.Map<TournamentDto>(updatedEntity);

        return new ApiOkResponse<TournamentDto>(updatedDto);
    }

    #endregion

    #region PUT Tournament details

    public async Task<ApiBaseResponse> UpdateAsync(int id, TournamentUpdateDto tournamentUpdateDto)
    {
        // Attempt to retrieve the tournament details by ID.
        TournamentDetails? tournamentDetails = await uoW.TournamentDetailsRepository.GetAsync(id);

        // If the tournament exists, map the DTO to the entity.
        if(tournamentDetails is null)
            return new TournamentNotFoundResponse($"Could not find tournament {id}.");

        // Map the update DTO to the existing tournament details entity.
        mapper.Map(tournamentUpdateDto, tournamentDetails);

        // UpdateAsync the tournament details in the repository.
        uoW.TournamentDetailsRepository.Update(tournamentDetails);

        // Persist the changes to the database.
        int success = await uoW.CompleteAsync();
        return success != 0
            ? new ApiOkResponse<TournamentDto>(mapper.Map<TournamentDto>(tournamentDetails))
            : new NoChangesMadeResponse($"The tournament, {tournamentUpdateDto.Title}, was not updated.");
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

    public async Task<ApiBaseResponse> RemoveAsync(int id)
    {
        // Attempt to retrieve the tournament details by ID.
        TournamentDetails? tournamentDetails = await uoW.TournamentDetailsRepository.GetAsync(id);

        if(tournamentDetails is null) {
            return new ApiNotFoundResponse($"Tournament with ID {id} was not found.");
        }

        bool hasGames = await uoW.TournamentDetailsRepository.HasGames(id);

        if(hasGames) {
            return new ApiConflictResponse($"Tournament with ID {id} has games associated and cannot be deleted.");
        }

        TournamentDto tournamentDto = mapper.Map<TournamentDto>(tournamentDetails);
        // Remove tournament and persist changes
        uoW.TournamentDetailsRepository.Remove(tournamentDetails);
        await uoW.CompleteAsync();

        return new ApiOkResponse<TournamentDto>(tournamentDto, "Tournament successfully deleted.");
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
