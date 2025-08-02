// -----------------------------------------------------------------------------
// File: ITournamentService.cs
// Summary: Defines the contract for tournament-related operations including 
//          retrieval, creation, update (full and partial), deletion, and 
//          existence checks. Designed to standardize access to business logic 
//          for tournament resources.
// Author: [Clive Leddy]
// Created: [2025-07-09]
// Notes:  All methods return wrapped <see cref="Domain.Models.Responses.ApiBaseResponse"/>
//         types to standardize API responses. Supports pagination, entity
//         existence checks, and safe deletions based on domain rules.
// -----------------------------------------------------------------------------

using Domain.Models.Responses;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Service.Contracts;

/// <summary>
/// Defines the contract for managing tournament entities, supporting operations
/// such as retrieval (single and paginated list), creation, full and partial updates,
/// deletion with business rule enforcement, and existence checks.
/// </summary>
public interface ITournamentService
{

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
    Task<(ApiBaseResponse tournamentDto, MetaData metaData)> GetAllAsync(TournamentRequestParameters requestParameters, bool trackChanges = false);

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
    Task<ApiBaseResponse> GetByIdAsync(int id, bool trackChanges = false);

    /// <summary>
    /// Creates a new tournament entry based on the provided details.
    /// </summary>
    /// <param name="tournamentDetailsCreateDto">The data transfer object containing the new tournament's information.</param>
    /// <returns>
    /// A tuple containing:
    /// - <c>id</c>: The ID of the newly created tournament.
    /// - <c>response</c>: An <see cref="ApiBaseResponse"/> wrapping the created tournament DTO and a success message.
    /// </returns>
    Task<(int id, ApiBaseResponse response)> CreateAsync(TournamentDetailsCreateDto tournamentDetailsCreateDto);

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
    Task<ApiBaseResponse> ApplyToAsync(int id, TournamentDto tournamentUpdateDto);

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
    Task<ApiBaseResponse> UpdateAsync(int id, TournamentUpdateDto tournamentUpdateDto);

    /// <summary>
    /// Checks whether a tournament exists with the specified title and start date.
    /// </summary>
    /// <param name="title">The title of the tournament to check.</param>
    /// <param name="startDate">The start date of the tournament to check.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> indicating whether the tournament exists.
    /// Returns <see cref="ApiOkResponse{bool}"/> with true if exists; otherwise, <see cref="ApiNotFoundResponse"/>.
    /// </returns>
    Task<ApiBaseResponse> ExistsAsync(string title, DateTime startDate);

    /// <summary>
    /// Checks whether a tournament exists with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tournament to check.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> indicating whether the tournament exists.
    /// Returns <see cref="ApiOkResponse{bool}"/> with true if exists; otherwise, <see cref="ApiNotFoundResponse"/>.
    /// </returns>
    Task<ApiBaseResponse> ExistsAsync(int id);

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
    Task<ApiBaseResponse> RemoveAsync(int id);
}