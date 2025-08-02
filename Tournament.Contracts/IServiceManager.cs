// -----------------------------------------------------------------------------
// File: IServiceManager.cs
// Summary: Defines an interface that aggregates access to core application services,
//          including tournament, game, and authentication services.
// Author: [Clive Leddy]
// Created: [2025-07-04]
// Last updated: [2025-08-02]
// Notes: Enables centralized and testable service management by exposing service
//        contracts through a unified interface. Supports lazy initialization
//        for improved performance and resource efficiency.
// -----------------------------------------------------------------------------

using Domain.Models.Responses;

namespace Service.Contracts;

/// <summary>
/// Defines a contract for managing and accessing core application services,
/// including tournament handling, game logic, and user authentication.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for providing concrete instances
/// of the service contracts, typically using lazy initialization to optimize
/// performance and resource management.
/// </remarks>
public interface IServiceManager
{
    /// <summary>
    /// Gets the service responsible for tournament-related operations.
    /// </summary>
    ITournamentService TournamentService { get; }

    /// <summary>
    /// Gets the service responsible for game-related logic and functionality.
    /// </summary>
    IGameService GameService { get; }

    /// <summary>
    /// Gets the service responsible for user authentication and authorization.
    /// </summary>
    IAuthService AuthService { get; }

    /// <summary>
    /// Checks if a game exists by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the game.</param>
    /// <returns>An <see cref="ApiBaseResponse"/> indicating whether the game exists.</returns>
    Task<ApiBaseResponse> DoesGameExist(int id);

    /// <summary>
    /// Checks if a tournament exists by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tournament.</param>
    /// <returns>An <see cref="ApiBaseResponse"/> indicating whether the tournament exists.</returns>
    Task<ApiBaseResponse> DoesTournamentExist(int id);
}