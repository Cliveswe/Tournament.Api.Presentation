// -----------------------------------------------------------------------------
// File: IGameRepository.cs
// Summary: Declares the contract for repository operations on Game entities,
//          including CRUD methods and game-specific queries such as by title,
//          date, and tournament association.
// Author: [Clive Leddy]
// Created: [2025-06-27]
// Notes: Defines asynchronous signatures for use in dependency injection and
//        separation of concerns. Implemented by GameRepository using EF Core.
// -----------------------------------------------------------------------------

using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;

/// <summary>
/// Provides repository methods for managing <see cref="Game"/> entities using Entity Framework Core.
/// </summary>
/// <remarks>
/// This class encapsulates CRUD operations and supports queries by attributes such as title, tournament association,
/// and scheduled date. It ensures efficient data access and modification using asynchronous patterns and LINQ expressions.
/// Key capabilities include:
/// <list type="bullet">
/// <item>Adding new game records.</item>
/// <item>Checking for existence by ID or combination of attributes.</item>
/// <item>Retrieving games by ID, title, date, or tournament association.</item>
/// <item>Updating and removing existing game entries.</item>
/// </list>
/// </remarks>
public interface IGameRepository
{
    /// <summary>
    /// Asynchronously retrieves all <see cref="Game"/> entities from the database.
    /// </summary>
    /// <returns>
    /// A task that yields a collection of all stored games.
    /// </returns>
    Task<IEnumerable<Game>> GetAllAsync(bool trackChanges = false);

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> by its unique identifier.
    /// </summary>
    /// <param name="gameId">The unique ID of the game.</param>
    /// <returns>
    /// A task with the corresponding <see cref="Game"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<Game?> GetByIdAsync(int gameId, bool trackChanges = false);

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> by its title using a case-insensitive comparison.
    /// </summary>
    /// <param name="gameTitle">The title of the game to search for.</param>
    /// <returns>
    /// A task that yields the matching game, or <c>null</c> if none is found.
    /// </returns>
    /// <remarks>
    /// Uses a lowercase string comparison for compatibility with EF Core's LINQ-to-SQL translation.
    /// </remarks>
    Task<Game?> GetByTitleAsync(string gameTitle);

    /// <summary>
    /// Provides repository methods for managing <see cref="Game"/> entities using Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// This class encapsulates CRUD operations and supports queries by attributes such as title, tournament association,
    /// and scheduled date. It ensures efficient data access and modification using asynchronous patterns and LINQ expressions.
    /// Key capabilities include:
    /// <list type="bullet">
    /// <item>Adding new game records.</item>
    /// <item>Checking for existence by ID or combination of attributes.</item>
    /// <item>Retrieving games by ID, title, date, or tournament association.</item>
    /// <item>Updating and removing existing game entries.</item>
    /// </list>
    /// </remarks>
    Task<bool> AnyAsync(int id);

    /// <summary>
    /// Determines whether a game with the specified title and scheduled time exists in the data store.
    /// </summary>
    /// <param name="name">The game's title to match.</param>
    /// <param name="date">The scheduled date and time of the game.</param>
    /// <returns>
    /// A task representing the asynchronous operation. Returns <see langword="true"/> if a matching game exists; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByNameAndDateAsync(string name, DateTime date);

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> by title and associated tournament ID.
    /// </summary>
    /// <param name="name">The title of the game to retrieve.</param>
    /// <param name="tournamentId">The unique ID of the tournament.</param>
    /// <returns>
    /// A task with a result of the matching <see cref="Game"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<Game?> GetByNameAndDateAsync(string name, int tournamentId, bool trackChanges = false);


    /// <summary>
    /// Retrieves all <see cref="Game"/> entities associated with a specific tournament.
    /// </summary>
    /// <param name="tournamentId">The tournament's unique identifier.</param>
    /// <returns>
    /// A task with a collection of all games linked to the specified tournament. Returns an empty list if none found.
    /// </returns>
    Task<IEnumerable<Game?>> GetByTournamentIdAsync(int tournamentId);

    /// <summary>
    /// Adds a new <see cref="Game"/> entity to the database context.
    /// </summary>
    /// <param name="game">The game to add. Must not be <c>null</c>.</param>
    /// <remarks>
    /// Marks the specified entity for insertion into the database on the next save operation.
    /// </remarks>
    void Add(Game game);

    /// <summary>
    /// Marks the specified <see cref="Game"/> as modified in the database context.
    /// </summary>
    /// <param name="game">The game entity to update. Must not be <c>null</c>.</param>
    /// <remarks>
    /// Sets the entity's state to <see cref="EntityState.Modified"/> to track changes for saving.
    /// </remarks>
    void Update(Game game);

    /// <summary>
    /// Removes the specified <see cref="Game"/> entity from the data store.
    /// </summary>
    /// <param name="game">The game to remove. Must not be <c>null</c>.</param>
    /// <remarks>
    /// Marks the entity for deletion on the next database save.
    /// </remarks>
    void Remove(Game game);
}
