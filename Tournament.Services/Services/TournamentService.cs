// Ignore Spelling: Dto

using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Responses;
using Service.Contracts;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Tournaments.Services.Services;
public class TournamentService(IMapper mapper, IUnitOfWork unitOfWork) : ITournamentService
{
    #region GET Tournament details
    // Methods for retrieving tournament information, including list queries and individual fetches.

    public async Task<(ApiBaseResponse tournamentDto, MetaData metaData)> GetAllAsync(TournamentRequestParameters requestParameters, bool trackChanges = false)
    {
        // Retrieves a paginated list of tournaments based on the given parameters.
        // Adjusts page number if it exceeds the total available pages.

        // Clamp request parameters to valid ranges
        TournamentRequestParameters clampedParameters =  ServiceManager.ClampRequestParameters(requestParameters);

        var pagedList = await unitOfWork
            .TournamentDetailsRepository
            .GetAllAsync(requestParameters, trackChanges);

        // If requested page number exceeds total pages, clamp and fetch again
        if(pagedList.MetaData.TotalPages > 0
            && clampedParameters.PageNumber > pagedList.MetaData.TotalPages) {

            clampedParameters.PageNumber = pagedList.MetaData.TotalPages;

            // Fetch again with corrected PageNumber
            pagedList = await unitOfWork
                .TournamentDetailsRepository
                .GetAllAsync(clampedParameters, trackChanges);
        }

        var tournamentDtos = mapper.Map<IEnumerable<TournamentDto>>(pagedList.Items);

        return tournamentDtos.Any()
            ? (new ApiOkResponse<IEnumerable<TournamentDto>>(tournamentDtos), pagedList.MetaData)
            : (new ApiTournamentNotFoundResponse("No tournaments found."), pagedList.MetaData);
    }

    public async Task<ApiBaseResponse> GetByIdAsync(int id, bool trackChanges = false)
    {
        // Fetches a tournament by its unique ID.
        TournamentDetails? tournamentDetails = await unitOfWork
            .TournamentDetailsRepository
            .GetAsync(id, trackChanges);

        if(tournamentDetails is null || tournamentDetails.Id != id) {
            return new ApiTournamentNotFoundResponse($"Tournament with ID {id} was not found.");
        }

        return new ApiOkResponse<TournamentDto>(mapper.Map<TournamentDto>(tournamentDetails));
    }

    #endregion

    #region PATCH Tournament details
    // Methods for partially updating tournament information using DTOs.

    public async Task<ApiBaseResponse> ApplyToAsync(int id, TournamentDto tournamentUpdateDto)
    {
        // Applies partial updates to an existing tournament entity.

        // Retrieve the existing tournament details.
        TournamentDetails? existingEntity = await unitOfWork.TournamentDetailsRepository.GetAsync(id, trackChanges: true);
        if(existingEntity is null) {
            return new ApiTournamentNotFoundResponse($"Tournament with id {id} was not found.");
        }

        // Map the incoming DTO to the existing tournament details.
        mapper.Map(tournamentUpdateDto, existingEntity);

        // UpdateAsync the existing tournament details in the repository.
        unitOfWork.TournamentDetailsRepository.Update(existingEntity);
        // Persist the changes to the database.
        int result = await unitOfWork.CompleteAsync();

        if(result == 0)
            return new ApiNotModifiedResponse("The tournament was not updated.");

        // Fetch fresh data from DB to reflect all updates
        TournamentDetails? updatedEntity = await unitOfWork.TournamentDetailsRepository.GetAsync(id, trackChanges: false);
        TournamentDto updatedDto = mapper.Map<TournamentDto>(updatedEntity);

        return new ApiOkResponse<TournamentDto>(updatedDto);
    }

    #endregion

    #region PUT Tournament details
    // Methods for full updates to tournament data.

    public async Task<ApiBaseResponse> UpdateAsync(int id, TournamentUpdateDto tournamentUpdateDto)
    {
        // Performs a full update of a tournament's details.

        // Attempt to retrieve the tournament details by ID.
        TournamentDetails? tournamentDetails = await unitOfWork.TournamentDetailsRepository.GetAsync(id);

        // If the tournament exists, map the DTO to the entity.
        if(tournamentDetails is null)
            return new ApiTournamentNotFoundResponse($"Could not find tournament {id}.");

        // Map the update DTO to the existing tournament details entity.
        mapper.Map(tournamentUpdateDto, tournamentDetails);

        // UpdateAsync the tournament details in the repository.
        unitOfWork.TournamentDetailsRepository.Update(tournamentDetails);

        // Persist the changes to the database.
        int success = await unitOfWork.CompleteAsync();

        return success != 0
            ? new ApiOkResponse<TournamentDto>(mapper.Map<TournamentDto>(tournamentDetails))
            : new ApiNoChangesMadeResponse($"The tournament, {tournamentUpdateDto.Title}, was not updated.");
    }

    #endregion

    #region POST Tournament details
    // Methods to create new tournament entries.

    public async Task<(int id, TournamentDto tournamentDto)> CreateAsync(TournamentDetailsCreateDto tournamentDetailsCreateDto)
    {
        // Creates a new tournament and returns its ID and DTO.

        // Map the DTO to the TournamentDetails entity.
        TournamentDetails tournamentDetails = mapper.Map<TournamentDetails>(tournamentDetailsCreateDto);
        // Add the new TournamentDetails entity to the repository
        unitOfWork.TournamentDetailsRepository.Add(tournamentDetails);
        // Persist the changes to the database
        await unitOfWork.CompleteAsync();
        // Return the ID of the newly created tournament and the mapped DTO as a tuple.
        return (tournamentDetails.Id, mapper.Map<TournamentDto>(tournamentDetails));

    }

    #endregion

    #region DELETE Tournament details
    // Methods to delete tournaments, ensuring no related games exist.

    public async Task<ApiBaseResponse> RemoveAsync(int id)
    {
        // Removes a tournament if no games are associated.

        // Attempt to retrieve the tournament details by ID.
        TournamentDetails? tournamentDetails = await unitOfWork.TournamentDetailsRepository.GetAsync(id);

        if(tournamentDetails is null) {
            return new ApiNotFoundResponse($"Tournament with ID {id} was not found.");
        }

        bool hasGames = await unitOfWork.TournamentDetailsRepository.HasGames(id);

        if(hasGames) {
            return new ApiConflictResponse($"Tournament with ID {id} has games associated and cannot be deleted.");
        }

        TournamentDto tournamentDto = mapper.Map<TournamentDto>(tournamentDetails);
        // Remove tournament and persist changes
        unitOfWork.TournamentDetailsRepository.Remove(tournamentDetails);
        await unitOfWork.CompleteAsync();

        return new ApiOkResponse<TournamentDto>(tournamentDto, "Tournament successfully deleted.");
    }

    #endregion

    #region Exists Tournament details
    // Methods to verify existence of tournaments by ID or by title and start date.

    public async Task<ApiBaseResponse> ExistsAsync(string title, DateTime startDate)
    {
        // Checks existence of a tournament by title and start date.

        bool entityExists = await unitOfWork.TournamentDetailsRepository
            .ExistsByTitleAndStartDateAsync(title, startDate);

        return entityExists
            ? new ApiOkResponse<bool>(entityExists)
            : new ApiNotFoundResponse($"Tournament {title} with start date {startDate} does not exists.");
    }

    public async Task<ApiBaseResponse> ExistsAsync(int id)
    {
        // Checks existence of a tournament by its ID.

        bool entityExists = await unitOfWork.TournamentDetailsRepository.AnyAsync(id);

        return entityExists
            ? new ApiOkResponse<bool>(entityExists)
            : new ApiNotFoundResponse("Tournament does not exists.");
    }

    #endregion
}
