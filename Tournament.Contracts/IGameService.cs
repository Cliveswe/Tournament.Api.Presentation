// -----------------------------------------------------------------------------
// File: IGameService.cs
// Summary: Defines the contract for business-layer operations on "Game" resources,
//          including create, retrieve, update, patch, and delete actions. Enforces
//          domain rules (such as max game limits) and returns standardized
//          <see cref="Domain.Models.Responses.ApiBaseResponse"/> results.
// Author: [Clive Leddy]
// Created: [2025-07-09]
// Notes:  Uses AutoMapper for DTO-to-entity mapping and the Unit-of-Work pattern
//         for data persistence. All public members include XML documentation
//         to support Swagger and IntelliSense.
// -----------------------------------------------------------------------------

using Domain.Models.Responses;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Service.Contracts;

/// <summary>
/// Service interface for all game-related operations (CRUD, patch, existence checks,
/// pagination, and business validations). Relies on <see cref="IUnitOfWork"/> for data access
/// and <see cref="IMapper"/> for mapping DTOs, returning standard
/// <see cref="ApiBaseResponse"/> objects to enable consistent error handling in controllers.
/// </summary>
/// <remarks>
/// Acts as a facade to hide repository complexity from controllers, providing a simple
/// interface and enforcing cross-entity business rules such as <see cref="MaxNumberOfGames"/>.
/// </remarks>
public interface IGameService
{
    /// <summary>
    /// Maximum number of games allowed inside a single tournament.
    /// Requests that would exceed this number yield an
    /// <see cref="ApiMaxGameLimitReachedResponse"/>.
    /// </summary>
    int MaxNumberOfGames { get; }


    /// <summary>
    /// Retrieves a paginated list of games associated with a specific tournament,
    /// applying any request parameters such as page number and size. If the page number
    /// exceeds the available total pages, it automatically adjusts the page and retries the query.
    /// </summary>
    /// <param name="requestParameters">Pagination and filtering parameters for the query.</param>
    /// <param name="tournamentId">The unique ID of the tournament to retrieve games from.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><see cref="ApiBaseResponse"/>: An <see cref="ApiOkResponse{T}"/> with the game data if successful, or an <see cref="ApiTournamentNotFoundResponse"/> if no games are found.</item>
    /// <item><see cref="MetaData"/>: Pagination metadata including current page, page size, total items, and total pages.</item>
    /// </list>
    /// </returns>
    Task<(ApiBaseResponse gameResponse, MetaData metaData)> GetGamesAsync(TournamentRequestParameters requestParameters, int tournamentId);

    /// <summary>
    /// Retrieves a game by its unique identifier within the specified tournament.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament that the game is expected to belong to.</param>
    /// <param name="id">The unique identifier of the game to retrieve.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> that contains either:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> with the game data if found and associated with the given tournament.</item>
    /// <item><see cref="ApiGameNotFoundByIdResponse"/> if no matching game is found or it does not belong to the specified tournament.</item>
    /// </list>
    /// </returns>
    Task<ApiBaseResponse> GetGameAsync(int tournamentId, int id);

    /// <summary>
    /// Retrieves a game by its title within the specified tournament.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament to search within.</param>
    /// <param name="title">The title of the game to retrieve.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> that contains either:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> with the game data if found.</item>
    /// <item><see cref="ApiGameNotFoundByTitleResponse"/> if no game with the given title exists in the tournament.</item>
    /// </list>
    /// </returns>
    Task<ApiBaseResponse> GetGameAsync(int tournamentId, string title);


    /// <summary>
    /// Adds a new game to the specified tournament if it doesn't already exist and the game limit hasn't been exceeded.
    /// </summary>
    /// <param name="gameCreateDto">The data transfer object containing the game details to be created.</param>
    /// <param name="tournamentId">The ID of the tournament to which the game should be added.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> representing the result of the add operation. Possible responses include:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> if the game was successfully added.</item>
    /// <item><see cref="ApiAlreadyExistsResponse"/> if a game with the same name already exists in the tournament.</item>
    /// <item><see cref="ApiMaxGameLimitReachedResponse"/> if the tournament has reached the maximum number of games allowed.</item>
    /// <item><see cref="ApiSaveFailedResponse"/> if the game could not be saved to the database.</item>
    /// </list>
    /// </returns>
    Task<ApiBaseResponse> AddGameAsync(GameCreateDto gameCreateDto, int tournamentId);

    /// <summary>
    /// Updates an existing game in a tournament using the provided title and update data.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament to which the game belongs.</param>
    /// <param name="title">The title of the game to be updated.</param>
    /// <param name="gameUpdateDto">The DTO containing updated properties for the game.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> representing the result of the update operation. Possible responses include:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> if the game is successfully updated.</item>
    /// <item><see cref="ApiGameNotFoundByTitleResponse"/> if the game is not found by title.</item>
    /// <item><see cref="ApiNoChangesMadeResponse"/> if no changes were applied during update.</item>
    /// </list>
    /// </returns>
    Task<ApiBaseResponse> UpdateAsync(int tournamentId, string title, GameUpdateDto gameUpdateDto);


    /// <summary>
    /// Applies updates from a <see cref="GameDto"/> to an existing game entity identified by its ID,
    /// validating that the game's start date falls within the specified tournament date range.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament the game belongs to.</param>
    /// <param name="id">The ID of the game to be updated.</param>
    /// <param name="gameDto">The DTO containing the updated game data.</param>
    /// <param name="tournamentDto">The DTO containing the tournament's date range for validation.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> representing the result of the operation. This may be:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> if the update is successful.</item>
    /// <item><see cref="ApiGameNotFoundByIdResponse"/> if the game could not be found.</item>
    /// <item><see cref="ApiUnProcessableContentResponse"/> if the date is out of range.</item>
    /// <item><see cref="ApiNotModifiedResponse"/> if no changes were made.</item>
    /// </list>
    /// </returns>
    Task<ApiBaseResponse> ApplyToAsync(int tournamentId, int id, GameDto gameDto, TournamentDto tournamentDto);

    /// <summary>
    /// Removes a game from the specified tournament by its ID.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament from which the game should be removed.</param>
    /// <param name="id">The ID of the game to remove.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> indicating the result of the removal operation. Possible responses include:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> if the game was successfully removed.</item>
    /// <item><see cref="ApiNotFoundResponse"/> if the game does not exist or does not belong to the specified tournament.</item>
    /// </list>
    /// </returns>
    Task<ApiBaseResponse> RemoveAsync(int tournamentId, int id);

    /// <summary>
    /// Checks if a game with the specified ID exists in the repository.
    /// </summary>
    /// <param name="id">The ID of the game to check for existence.</param>
    /// <returns>
    /// An <see cref="ApiOkResponse{bool}"/> with value true if the game exists; 
    /// otherwise, an <see cref="ApiNotFoundResponse"/> indicating the game was not found.
    /// </returns>
    Task<ApiBaseResponse> ExistsAsync(int id);
}

