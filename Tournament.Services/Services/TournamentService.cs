// Ignore Spelling: Dto

// -----------------------------------------------------------------------------
// File: TournamentService.cs
// Summary: Implements business-layer operations for “Tournament” resources – 
//          including creation, retrieval, update, patch, deletion, and existence 
//          checks. Handles logic like clamped pagination and safe deletion, 
//          and returns rich <see cref="Domain.Models.Responses.ApiBaseResponse"/> 
//          results.
// Author: [Clive Leddy]
// Created: [2025-07-09]
// Last Modified:  [2025-08-02]
// Notes:  Uses AutoMapper for DTO↔entity mapping and the Unit-of-Work pattern 
//         for data access. Grouped and documented using region blocks for 
//         CRUD clarity. Fully XML-documented for Swagger / IntelliSense.
// -----------------------------------------------------------------------------

using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Responses;
using Service.Contracts;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Tournaments.Services.Services;

/// <summary>
/// Provides operations for managing tournament data, including creation, retrieval, updating, 
/// deletion, and existence checks. This service acts as an intermediary between the data access 
/// layer and higher-level application logic, ensuring business rules are applied consistently.
/// </summary>
public class TournamentService(IMapper mapper, IUnitOfWork unitOfWork) : ITournamentService
{
    #region GET Tournament details
    // Methods for retrieving tournament information, including list queries and individual fetches.

    /// <summary>
    /// Retrieves a paginated list of tournaments based on the provided request parameters.
    /// </summary>
    /// <param name="requestParameters">Filtering and pagination parameters used to shape the result set.</param>
    /// <param name="trackChanges">Indicates whether to track changes on the retrieved entities.</param>
    /// <returns>
    /// A tuple containing:
    /// - An <see cref="ApiBaseResponse"/> that wraps a collection of <see cref="TournamentDto"/>s if any exist, 
    ///   or an <see cref="ApiTournamentNotFoundResponse"/> if none were found.
    /// - A <see cref="MetaData"/> object providing pagination details for the result set.
    /// </returns>
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

    /// <summary>
    /// Retrieves a tournament by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tournament.</param>
    /// <param name="trackChanges">Indicates whether to enable change tracking on the entity.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> containing:
    /// - <see cref="ApiOkResponse{T}"/> with the tournament data if found.
    /// - <see cref="ApiTournamentNotFoundResponse"/> if the tournament does not exist.
    /// </returns>
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

    /// <summary>
    /// Applies partial updates to an existing tournament entity using a provided DTO.
    /// </summary>
    /// <param name="id">The unique identifier of the tournament to update.</param>
    /// <param name="tournamentUpdateDto">The DTO containing the updated tournament information.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> indicating the result of the partial update:
    /// - <see cref="ApiOkResponse{T}"/> with the updated tournament data if changes were applied.
    /// - <see cref="ApiTournamentNotFoundResponse"/> if the tournament was not found.
    /// - <see cref="ApiNotModifiedResponse"/> if no updates were saved.
    /// </returns>
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

    /// <summary>
    /// Updates an existing tournament with the provided details.
    /// </summary>
    /// <param name="id">The unique identifier of the tournament to update.</param>
    /// <param name="tournamentUpdateDto">The DTO containing the updated tournament information.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> indicating the outcome of the update operation:
    /// - <see cref="ApiOkResponse{T}"/> with the updated tournament data if successful.
    /// - <see cref="ApiTournamentNotFoundResponse"/> if the tournament was not found.
    /// - <see cref="ApiNoChangesMadeResponse"/> if no changes were applied.
    /// </returns>
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

    /// <summary>
    /// Creates a new tournament entry based on the provided details.
    /// </summary>
    /// <param name="tournamentDetailsCreateDto">The data transfer object containing the new tournament's information.</param>
    /// <returns>
    /// A tuple containing:
    /// - <c>id</c>: The ID of the newly created tournament.
    /// - <c>tournamentDto</c>: The mapped DTO representing the created tournament.
    /// </returns>
    public async Task<(int id, ApiBaseResponse response)> CreateAsync(TournamentDetailsCreateDto tournamentDetailsCreateDto)
    {
        // Creates a new tournament and returns its ID and DTO.

        // Map the DTO to the TournamentDetails entity.
        TournamentDetails tournamentDetails = mapper.Map<TournamentDetails>(tournamentDetailsCreateDto);

        // Add the new TournamentDetails entity to the repository
        unitOfWork.TournamentDetailsRepository.Add(tournamentDetails);

        // Persist the changes to the database
        await unitOfWork.CompleteAsync();

        // Return the ID of the newly created tournament and the mapped DTO as a tuple.
        var tournamentDto = mapper.Map<TournamentDto>(tournamentDetails);
        // Create response object
        var response = new ApiOkResponse<TournamentDto>(tournamentDto, "Tournament successfully created.");

        return (tournamentDetails.Id, response);

    }

    #endregion

    #region DELETE Tournament details
    // Methods to delete tournaments, ensuring no related games exist.

    /// <summary>
    /// Deletes a tournament by its ID, if it exists and has no associated games.
    /// </summary>
    /// <param name="id">The unique identifier of the tournament to delete.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> indicating the outcome:
    /// - <see cref="ApiOkResponse{TournamentDto}"/> if successfully deleted,
    /// - <see cref="ApiNotFoundResponse"/> if the tournament does not exist,
    /// - <see cref="ApiConflictResponse"/> if the tournament has associated games.
    /// </returns>
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

    /// <summary>
    /// Checks whether a tournament exists with the specified title and start date.
    /// </summary>
    /// <param name="title">The title of the tournament to check.</param>
    /// <param name="startDate">The start date of the tournament to check.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> indicating whether the tournament exists.
    /// Returns <see cref="ApiOkResponse{bool}"/> with true if exists; otherwise, <see cref="ApiNotFoundResponse"/>.
    /// </returns>
    public async Task<ApiBaseResponse> ExistsAsync(string title, DateTime startDate)
    {
        // Checks existence of a tournament by title and start date.

        bool entityExists = await unitOfWork.TournamentDetailsRepository
            .ExistsByTitleAndStartDateAsync(title, startDate);

        return entityExists
            ? new ApiOkResponse<bool>(entityExists)
            : new ApiNotFoundResponse($"Tournament {title} with start date {startDate} does not exists.");
    }

    /// <summary>
    /// Checks whether a tournament exists with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tournament to check.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> indicating whether the tournament exists.
    /// Returns <see cref="ApiOkResponse{bool}"/> with true if exists; otherwise, <see cref="ApiNotFoundResponse"/>.
    /// </returns>
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
