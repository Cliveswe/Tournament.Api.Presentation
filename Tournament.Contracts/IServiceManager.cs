// -----------------------------------------------------------------------------
// File: IServiceManager.cs
// Summary: Defines an interface that aggregates access to core application services,
//          including tournament, game, and authentication services.
// Author: [Clive Leddy]
// Created: [2025-07-04]
// Notes: Enables centralized and testable service management by exposing service
//        contracts through a unified interface.
// -----------------------------------------------------------------------------


namespace Service.Contracts;

/// <summary>
/// Defines a contract for managing and accessing core services in the application,
/// including tournament handling, game logic, and user authentication.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for providing
/// concrete instances of the service contracts, typically using
/// lazy initialization for performance and resource efficiency.
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

    Task<bool> DoesGameExist(int id);
    Task<bool> DoesTournamentExist(int id);
}