// Ignore Spelling: Auth

// -----------------------------------------------------------------------------
// File: ServiceManager.cs
// Summary: Provides centralized and lazy-loaded access to application services
//          including tournament, game, and authentication services.
// Author: [Clive Leddy]
// Created: [2025-07-04]
// Notes: Implements the IServiceManager interface to aggregate service dependencies,
//        improving modularity, testability, and life-cycle management.
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

    public async Task<ApiBaseResponse> DoesGameExist(int id) => await gameService.Value.ExistsAsync(id);

    public async Task<ApiBaseResponse> DoesTournamentExist(int id) => await tournamentService.Value.ExistsAsync(id);

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

    /// <summary>
    /// Gets the game service instance for managing game-specific logic and operations.
    /// </summary>
    public IGameService GameService => gameService.Value;

    /// <summary>
    /// Gets the authentication service instance for handling user authentication and authorization logic.
    /// </summary>
    public IAuthService AuthService => authService.Value;
}
