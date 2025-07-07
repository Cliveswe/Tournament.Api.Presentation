// ------------------------------------------------------------------------------------------------
// File: GameRepository.cs
// Summary: Implements repository methods for managing Game entities in the database,
//          including CRUD operations and queries by game attributes or tournament association.
//          Supports asynchronous operations for fetching, adding, updating, and removing games.
//          Allows optional tracking of entity changes via the trackChanges parameter.
// Author: [Clive Leddy]
// Created: [2025-06-27]
// Notes: Uses Entity Framework Core for data access via the TournamentApiContext. 
//        Ensures data consistency by tracking entity states and querying with LINQ.
//        The trackChanges parameter controls EF Core change tracking behavior for queries.
// ------------------------------------------------------------------------------------------------


using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;

/// <summary>
/// Provides a repository implementation for managing <see cref="Game"/> entities within the data access layer.
/// </summary>
/// <remarks>
/// Implements the <see cref="IGameRepository"/> interface and inherits from <see cref="RepositoryBase{Game}"/>.
/// This class encapsulates all data operations for games, including CRUD (Create, Read, Update, Delete) and
/// query methods. It interacts with the database via Entity Framework Core using the <see cref="TournamentApiContext"/>.
///
/// Supports asynchronous operations for improved scalability and responsiveness. Includes helper methods
/// for retrieving games by ID, title, and tournament association, as well as checks for existence based on
/// game attributes.
///
/// Designed following the Repository pattern to ensure separation of concerns and improve maintainability
/// and testability of the data access layer.
/// </remarks>
/// <example>
/// Example usage:
/// <code>
/// var gameRepo = new GameRepository(context);
/// var allGames = await gameRepo.GetAllAsync();
/// var exists = await gameRepo.ExistsByNameAndDateAsync("Chess", DateTime.Today);
/// </code>
/// </example>
public class GameRepository(TournamentApiContext context) : RepositoryBase<Game>(context), IGameRepository
{

    /// <summary>
    /// Adds a new <see cref="Game"/> entity to the data store.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> object to add. Must not be <c>null</c>.</param>
    /// <remarks>
    /// This method stages the game for insertion into the database context. The changes will be persisted
    /// to the database when <c>SaveChanges</c> or <c>SaveChangesAsync</c> is called on the context.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="game"/> parameter is <c>null</c>.</exception>
    public void Add(Game game)
    {
        Create(game);
    }

    /// <summary>
    /// Asynchronously checks whether a <see cref="Game"/> entity with the specified ID exists in the data store.
    /// </summary>
    /// <param name="id">The unique identifier of the game to check.</param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> representing the asynchronous operation. The task result is <c>true</c>
    /// if a game with the specified ID exists; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method uses a non-tracking query for improved performance, as it is used only to check existence.
    /// </remarks>
    public async Task<bool> AnyAsync(int id)
    {
        return await FindByCondition(g => g.Id.Equals(id), false)
            .AnyAsync();
    }

    /// <summary>
    /// Asynchronously determines whether a <see cref="Game"/> entity exists with the specified title and date.
    /// </summary>
    /// <param name="name">The title of the game to check for.</param>
    /// <param name="date">The exact date and time of the game to match.</param>
    /// <param name="trackChanges">
    /// A boolean indicating whether the query should track changes to the returned entity in the context.
    /// Use <c>false</c> for read-only operations to improve performance.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> representing the asynchronous operation. The task result is <c>true</c>
    /// if a game with the specified title and date exists; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method performs a case-sensitive match on the game's title and an exact match on the <c>Time</c> property.
    /// </remarks>
    public async Task<bool> ExistsByNameAndDateAsync(string name, DateTime date, bool trackChanges = false)
    {
        return await FindByCondition(g => g.Title.Equals(name)
        && g.Time == date, trackChanges)
            .AnyAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its title and associated tournament ID.
    /// </summary>
    /// <param name="name">The title of the game to retrieve.</param>
    /// <param name="tournamentId">The identifier of the tournament to which the game belongs.</param>
    /// <param name="trackChanges">
    /// A boolean indicating whether the query should track changes to the returned entity in the context.
    /// Set to <c>false</c> for read-only operations to improve performance.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Game}"/> representing the asynchronous operation. The result is the matching <see cref="Game"/>
    /// entity if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method performs an exact (case-sensitive) match on the game title and matches the specified tournament ID.
    /// </remarks>
    public async Task<Game?> GetByNameAndDateAsync(string name, int tournamentId, bool trackChanges = false)
    {
        return await FindByCondition(g => g.Title == name && g.TournamentDetailsId == tournamentId, trackChanges)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Asynchronously retrieves all <see cref="Game"/> entities from the data store, ordered by title.
    /// </summary>
    /// <param name="trackChanges">
    /// A boolean value indicating whether change tracking should be enabled for the retrieved entities.
    /// Set to <c>false</c> for read-only operations to improve performance.
    /// </param>
    /// <returns>
    /// A <see cref="Task{IEnumerable{Game}}"/> representing the asynchronous operation. 
    /// The result contains a collection of all <see cref="Game"/> entities, ordered alphabetically by title.
    /// </returns>
    /// <remarks>
    /// This method queries the database for all games and returns them as an ordered list. 
    /// Use <paramref name="trackChanges"/> to control whether EF Core should track entity changes.
    /// </remarks>
    public async Task<IEnumerable<Game>> GetAllAsync(bool trackChanges = false)
    {
        return await FindAll(trackChanges)
            .OrderBy(g => g.Title)
            .ToListAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its unique identifier.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to retrieve.</param>
    /// <param name="trackChanges">
    /// A boolean indicating whether change tracking should be enabled for the retrieved entity.
    /// Set to <c>false</c> to improve performance for read-only access.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Game}"/> representing the asynchronous operation. 
    /// The result contains the <see cref="Game"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method performs a database query to fetch the game with the specified <paramref name="gameId"/>.
    /// Use <paramref name="trackChanges"/> to control whether the context tracks the returned entity.
    /// </remarks>
    public async Task<Game?> GetByIdAsync(int gameId, bool trackChanges = false)
    {
        return await FindByCondition(g => g.Id == gameId, trackChanges)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its title.
    /// </summary>
    /// <param name="gameTitle">The title of the game to retrieve. Case-sensitive match is used.</param>
    /// <returns>
    /// A <see cref="Task{Game}"/> representing the asynchronous operation.  
    /// The result contains the <see cref="Game"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method performs a case-sensitive search using the exact <paramref name="gameTitle"/> value.  
    /// Use this method when you know the exact title of the game you want to retrieve.
    /// </remarks>
    public async Task<Game?> GetByTitleAsync(string gameTitle)
    {
        Game? game = await FindByCondition(g => g.Title.Equals(gameTitle))
            .FirstOrDefaultAsync();
        return game;
    }

    /// <summary>
    /// Removes the specified <see cref="Game"/> entity from the data store.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> entity to remove. Cannot be <c>null</c>.</param>
    /// <remarks>
    /// This method marks the provided entity for deletion. Changes are applied to the database
    /// when the context is saved. Ensure the entity exists and is tracked by the context
    /// before calling this method.
    /// </remarks>
    public void Remove(Game game)
    {
        Delete(game);
    }

    /// <summary>
    /// Updates the specified <see cref="Game"/> entity in the data store.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> entity to update. Cannot be <c>null</c>.</param>
    /// <remarks>
    /// This method marks the provided entity as modified. Changes are persisted to the database
    /// when the context is saved. Ensure the entity contains valid and updated information
    /// before calling this method.
    /// </remarks>
    public void Update(Game game)
    {
        Update(game);
    }

    /// <summary>
    /// Asynchronously retrieves all <see cref="Game"/> entities associated with a specific tournament.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament whose games are to be retrieved.</param>
    /// <param name="trackChanges">
    /// A boolean indicating whether to track changes on the retrieved entities. 
    /// If <c>true</c>, the entities are tracked by the context; otherwise, they are not.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an <see cref="IEnumerable{Game}"/> 
    /// of games related to the specified tournament, ordered by their title.
    /// </returns>
    public async Task<IEnumerable<Game?>> GetByTournamentIdAsync(int tournamentId, bool trackChanges = false)
    {
        return await FindByCondition(g => g.TournamentDetailsId == tournamentId, trackChanges)
            .OrderBy(g => g.Title)
            .ToListAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its title and associated tournament ID.
    /// </summary>
    /// <param name="gameTitle">The exact title of the game to retrieve. Case-sensitive match is used.</param>
    /// <param name="tournamentId">The unique identifier of the tournament to which the game belongs.</param>
    /// <param name="trackChanges">
    /// Indicates whether to enable change tracking for the retrieved entity.
    /// Set to <c>false</c> to improve performance in read-only scenarios.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Game}"/> representing the asynchronous operation. 
    /// The result contains the <see cref="Game"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method performs an exact (case-sensitive) match on the game's title and validates
    /// that the game belongs to the specified tournament. It supports optional change tracking via EF Core.
    /// </remarks>
    public Task<Game?> GetByTitleAndTournamentIdAsync(string gameTitle, int tournamentId, bool trackChanges = false)
    {
        return FindByCondition(g => g.Title.Equals(gameTitle) && g.TournamentDetailsId == tournamentId, trackChanges)
            .FirstOrDefaultAsync();
    }
}
