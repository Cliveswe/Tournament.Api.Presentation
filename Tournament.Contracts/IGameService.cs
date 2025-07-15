using Domain.Models.Responses;
using Service.Contracts.Enums;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Service.Contracts;

/// <summary>
/// Provides service methods for managing games within tournaments.
/// </summary>
public interface IGameService
{
    /// <summary>
    /// Retrieves all games for a specified tournament.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament.</param>
    /// <returns>A collection of <see cref="GameDto"/> objects belonging to the tournament.</returns>
    //Task<IEnumerable<GameDto>> GetAllAsync(int tournamentId);
    //Task<(IEnumerable<GameDto> gameDtos, MetaData metaData)> GetAllAsync(TournamentRequestParameters requestParameters, int tournamentId);

    Task<(ApiBaseResponse gameResponse, MetaData metaData)> GetGamesAsync(TournamentRequestParameters requestParameters, int tournamentId);
    Task<ApiBaseResponse> GetGameAsync(int tournamentId, int id);
    Task<ApiBaseResponse> GetGameAsync(int tournamentId, string title);

    /// <summary>
    /// Retrieves a game by its identifier within a specific tournament.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament.</param>
    /// <param name="id">The unique identifier of the game.</param>
    /// <returns>The <see cref="GameDto"/> if found; otherwise, <c>null</c>.</returns>
    Task<GameDto?> GetAsync(int tournamentId, int id);

    /// <summary>
    /// Retrieves a game by its title within a specific tournament.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament.</param>
    /// <param name="title">The title of the game.</param>
    /// <returns>The <see cref="GameDto"/> if found; otherwise, <c>null</c>.</returns>
    //Task<GameDto> GetAsync(int tournamentId, string title);

    /// <summary>
    /// Adds a new game to a tournament.
    /// </summary>
    /// <param name="gameCreateDto">The data transfer object containing the details of the game to add.</param>
    /// <param name="tournamentId">The unique identifier of the tournament to which the game belongs.</param>
    /// <returns>A tuple indicating success status, duplication status, and the added <see cref="GameDto"/> if successful.</returns>
    //Task<(bool isSuccess, bool isDuplicate, GameDto? gameDto)> AddAsync(GameCreateDto gameCreateDto, int tournamentId);
    Task<ApiBaseResponse> AddAsync(GameCreateDto gameCreateDto, int tournamentId);

    /// <summary>
    /// Updates an existing game identified by its title within a tournament.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament.</param>
    /// <param name="title">The title of the game to update.</param>
    /// <param name="gameUpdateDto">The data transfer object containing updated game information.</param>
    /// <returns>An <see cref="UpdateGameResult"/> indicating the result of the update operation.</returns>
    Task<UpdateGameResult> UpdateAsync(int tournamentId, string title, GameUpdateDto gameUpdateDto);

    /// <summary>
    /// Applies a JSON Patch document to a game within a tournament and validates the update.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament.</param>
    /// <param name="id">The unique identifier of the game to patch.</param>
    /// <param name="gameDto">The current <see cref="GameDto"/> representing the game.</param>
    /// <param name="tournamentDto">The <see cref="TournamentDto"/> representing the tournament details.</param>
    /// <returns>An <see cref="ApplyPatchResult"/> indicating the outcome of the patch operation.</returns>
    Task<ApplyPatchResult> ApplyToAsync(int tournamentId, int id, GameDto gameDto, TournamentDto tournamentDto);

    /// <summary>
    /// Removes a game from a tournament by its identifier.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament.</param>
    /// <param name="id">The unique identifier of the game to remove.</param>
    /// <returns><c>true</c> if the game was successfully removed; otherwise, <c>false</c>.</returns>
    Task<bool> RemoveAsync(int tournamentId, int id);

    /// <summary>
    /// Checks if a game exists by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the game.</param>
    /// <returns><c>true</c> if the game exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(int id);
}

