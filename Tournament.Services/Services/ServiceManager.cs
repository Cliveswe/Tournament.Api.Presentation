// Ignore Spelling: Auth

// -----------------------------------------------------------------------------
// File: ServiceManager.cs
// Summary: Provides centralized and lazy-loaded access to application services
//          including tournament, game, and authentication services.
// Author: [Clive Leddy]
// Created: [2025-07-04]
// Last updated: [2025-08-02]
// Notes: Implements the IServiceManager interface to aggregate service dependencies,
//        improving modularity, testability, and life-cycle management. Uses lazy
//        loading to optimize resource utilization by deferring service instantiation.
// -----------------------------------------------------------------------------

using Domain.Models.Responses;
using Service.Contracts;
using Tournaments.Shared.Request;

namespace Tournaments.Services.Services;

/// <summary>
/// Manages the creation and lifetime of core services used throughout the application,
/// including tournament, game, and authentication services. 
/// Uses lazy loading to defer instantiation until a service is requested, optimizing resource use.
/// </summary>
/// <remarks>
/// This class centralizes access to related services and decouples dependent components 
/// from direct service instantiation, improving testability and maintainability.
/// </remarks>
public class ServiceManager(
    Lazy<ITournamentService> tournamentService,
    Lazy<IGameService> gameService,
    Lazy<IAuthService> authService
) : IServiceManager
{
    /// <summary>
    /// Gets the tournament service instance for handling tournament-related operations.
    /// </summary>
    public ITournamentService TournamentService => tournamentService.Value;

    /// <summary>
    /// Gets the game service instance for managing game-specific logic and operations.
    /// </summary>
    public IGameService GameService => gameService.Value;

    /// <summary>
    /// Gets the authentication service instance for handling user authentication and authorization logic.
    /// </summary>
    public IAuthService AuthService => authService.Value;

    /// <summary>
    /// Checks if a game with the specified ID exists by delegating to the game service.
    /// </summary>
    /// <param name="id">The unique identifier of the game to check.</param>
    /// <returns>An <see cref="ApiBaseResponse"/> indicating whether the game exists.</returns>
    public async Task<ApiBaseResponse> DoesGameExist(int id) => await gameService.Value.ExistsAsync(id);

    /// <summary>
    /// Checks if a tournament with the specified ID exists by delegating to the tournament service.
    /// </summary>
    /// <param name="id">The unique identifier of the tournament to check.</param>
    /// <returns>An <see cref="ApiBaseResponse"/> indicating whether the tournament exists.</returns>
    public async Task<ApiBaseResponse> DoesTournamentExist(int id) => await tournamentService.Value.ExistsAsync(id);

    /// <summary>
    /// Clamps the pagination parameters in the given <see cref="TournamentRequestParameters"/>
    /// to valid ranges, ensuring page size and number are within allowed limits.
    /// </summary>
    /// <param name="requestParameters">The original tournament request parameters to clamp.</param>
    /// <returns>A new <see cref="TournamentRequestParameters"/> instance with clamped values.</returns>
    public static TournamentRequestParameters ClampRequestParameters(TournamentRequestParameters requestParameters)
    {
        // Clamp page size using the setter logic in RequestParameters
        return new TournamentRequestParameters
        {
            PageSize = requestParameters.PageSize,
            PageNumber = requestParameters.PageNumber,
            IncludeGames = requestParameters.IncludeGames
        };
    }

}
