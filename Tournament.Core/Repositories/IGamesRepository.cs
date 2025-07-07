// -----------------------------------------------------------------------------
// File: IGameRepository.cs
// Summary: Declares the contract for repository operations on Game entities,
//          including CRUD methods and game-specific queries such as by title,
//          date, and tournament association. Supports optional tracking of entity
//          changes via the trackChanges parameter in asynchronous methods.
// Author: [Clive Leddy]
// Created: [2025-06-27]
// Notes: Defines asynchronous signatures for use in dependency injection and
//        separation of concerns. Implemented by GameRepository using EF Core.
//        The trackChanges parameter allows control over EF Core change tracking.
// -----------------------------------------------------------------------------

using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;

/// <summary>
/// Defines the contract for repository operations on <see cref="Game"/> entities.
/// </summary>
/// <remarks>
/// This interface declares asynchronous CRUD methods and game-specific queries such as retrieval by title,
/// date, and tournament association. It is designed to be implemented by a repository class (e.g., <see cref="GameRepository"/>)
/// that uses Entity Framework Core for data access. The interface supports dependency injection and promotes
/// separation of concerns within the application architecture.
/// </remarks>
public interface IGameRepository
{

    /// <summary>
    /// Asynchronously retrieves all <see cref="Game"/> entities, optionally tracking changes.
    /// </summary>
    /// <param name="trackChanges">Indicates whether entity changes should be tracked.</param>
    /// <returns>A task containing an enumerable of all <see cref="Game"/> entities ordered by title.</returns>
    Task<IEnumerable<Game>> GetAllAsync(bool trackChanges = false);

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its unique identifier.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <param name="trackChanges">Indicates whether entity changes should be tracked.</param>
    /// <returns>A task containing the <see cref="Game"/> entity if found; otherwise, <c>null</c>.</returns>
    Task<Game?> GetByIdAsync(int gameId, bool trackChanges = false);

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its title.
    /// </summary>
    /// <param name="gameTitle">The title of the game to retrieve.</param>
    /// <returns>A task containing the <see cref="Game"/> entity if found; otherwise, <c>null</c>.</returns>
    Task<Game?> GetByTitleAsync(string gameTitle);

    /// <summary>
    /// Asynchronously checks whether a <see cref="Game"/> entity exists by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the game.</param>
    /// <returns>A task containing <c>true</c> if the game exists; otherwise, <c>false</c>.</returns>
    Task<bool> AnyAsync(int id);

    /// <summary>
    /// Asynchronously checks whether a <see cref="Game"/> exists with the specified name and date.
    /// </summary>
    /// <param name="name">The name of the game.</param>
    /// <param name="date">The date associated with the game.</param>
    /// <param name="trackChanges">Indicates whether entity changes should be tracked.</param>
    /// <returns>A task containing <c>true</c> if a matching game exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByNameAndDateAsync(string name, DateTime date, bool trackChanges = false);

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its name and associated tournament identifier.
    /// </summary>
    /// <param name="name">The name of the game.</param>
    /// <param name="tournamentId">The unique identifier of the tournament.</param>
    /// <param name="trackChanges">Indicates whether entity changes should be tracked.</param>
    /// <returns>A task containing the <see cref="Game"/> entity if found; otherwise, <c>null</c>.</returns>
   //Task<Game?> GetByNameAndTournamentAsync(string name, int tournamentId, bool trackChanges = false);

    /// <summary>
    /// Asynchronously retrieves all <see cref="Game"/> entities associated with a specific tournament.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament.</param>
    /// <param name="trackChanges">Indicates whether entity changes should be tracked.</param>
    /// <returns>A task containing an enumerable of <see cref="Game"/> entities ordered by title.</returns>
    Task<IEnumerable<Game?>> GetByTournamentIdAsync(int tournamentId, bool trackChanges = false);

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its title and associated tournament ID.
    /// </summary>
    /// <param name="gameTitle">The exact title of the game to retrieve. Case-sensitive match is used.</param>
    /// <param name="tournamentId">The unique identifier of the tournament to which the game belongs.</param>
    /// <param name="trackChanges">If <c>true</c>, enables EF Core change tracking for the returned entity; otherwise, disables tracking.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the matching <see cref="Game"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<Game?> GetByTitleAndTournamentIdAsync(string gameTitle, int tournamentId, bool trackChanges = false);

    /// <summary>
    /// Adds a new <see cref="Game"/> entity to the data store.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> entity to add.</param>
    void Add(Game game);

    /// <summary>
    /// Updates an existing <see cref="Game"/> entity in the data store.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> entity to update.</param>
    void Update(Game game);

    /// <summary>
    /// Removes an existing <see cref="Game"/> entity from the data store.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> entity to remove.</param>
    void Remove(Game game);
}
