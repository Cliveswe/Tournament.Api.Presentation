// ------------------------------------------------------------------------------------------------
// File: GameRepository.cs
// Summary: Implements repository methods for managing Game entities in the database,
//          including CRUD operations and queries by game attributes or tournament association.
//          Supports asynchronous operations for fetching, adding, updating, and removing games.
// Author: [Clive Leddy]
// Created: [2025-06-27]
// Notes: Uses Entity Framework Core for data access via the TournamentApiContext. 
//        Ensures data consistency by tracking entity states and querying with LINQ.
// ------------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;

/// <summary>
/// Provides a repository implementation for managing <see cref="Game"/> entities in the application's data layer.
/// Utilizes Entity Framework Core to perform asynchronous Create, Read, Update, and Delete (CRUD) operations
/// on game records, including filtering by attributes such as title, date, and tournament association.
/// </summary>
/// <remarks>
/// Key responsibilities include:
/// - Adding, updating, and removing <see cref="Game"/> entities in the database context.
/// - Querying games by unique identifiers, title and date combinations, or tournament ID.
/// - Checking for the existence of games to avoid duplication.
/// - Supporting efficient data access using LINQ and EF Core’s change tracking.
///
/// This class encapsulates data access logic, promoting separation of concerns, testability,
/// and data consistency across the application.
/// </remarks>
/// 
public class GameRepository(TournamentApiContext context) : RepositoryBase<Game>(context), IGameRepository
{
    /// <summary>
    /// Adds the specified game to the database context for tracking and persistence.
    /// </summary>
    /// <remarks>This method adds the game entity to the database context, marking it for insertion into the
    /// database upon saving changes. Ensure that the game object is properly initialized before calling this
    /// method.</remarks>
    /// <param name="game">The game to add. Cannot be null.</param>
    public void Add(Game game)
    {
        //context.Game.Add(game);
        Create(game);
    }

    /// <summary>
    /// Determines whether any game exists with the specified identifier.
    /// </summary>
    /// <remarks>This method performs an asynchronous query against the underlying data source to check for
    /// the existence of a game with the given identifier.</remarks>
    /// <param name="id">The unique identifier of the game to search for.</param>
    /// <returns><see langword="true"/> if a game with the specified identifier exists; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> AnyAsync(int id)
    {
        //return await context.Game.AnyAsync(g => g.Id == id);
        return await FindByCondition(g => g.Id.Equals(id), false)
            .AnyAsync();
    }


    /// <summary>
    /// Determines whether a game with the specified title and date exists in the database.
    /// </summary>
    /// <param name="name">The title of the game to search for.</param>
    /// <param name="date">The date and time of the game to match.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains <see langword="true"/> 
    /// if a game with the specified title and date exists; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method is typically used to prevent the creation of duplicate game entries 
    /// by checking for existing records with the same title and scheduled time.
    /// Ensure that the title and date values are normalized before calling this method 
    /// to avoid false negatives due to formatting differences.
    /// </remarks>
    public async Task<bool> ExistsByNameAndDateAsync(string name, DateTime date, bool trackChanges = false)
    {
        //return await context.Game.AnyAsync(g => g.Title == name && g.Time == date);
        return await FindByCondition(g => g.Title.Equals(name)
        && g.Time == date, trackChanges)
            .AnyAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a game by its title and associated tournament identifier.
    /// </summary>
    /// <param name="name">The title of the game to retrieve.</param>
    /// <param name="tournamentId">The unique identifier of the tournament the game belongs to.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the <see cref="Game"/>
    /// object matching the specified title and tournament ID if found; otherwise, <c>null</c>.
    /// </returns>
    public async Task<Game?> GetByNameAndDateAsync(string name, int tournamentId, bool trackChanges = false)
    {
        //return await context.Game
        //    .FirstOrDefaultAsync(g => g.Title == name && g.TournamentDetailsId == tournamentId);

        return await FindByCondition(g => g.Title == name && g.TournamentDetailsId == tournamentId, trackChanges)
            .FirstOrDefaultAsync();
    }


    /// <summary>
    /// Asynchronously retrieves all games from the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of  <see cref="Game"/>
    /// objects representing all games in the database.</returns>
    public async Task<IEnumerable<Game>> GetAllAsync(bool trackChanges = false)
    {
        //return await context.Game.ToListAsync();
        return await FindAll(trackChanges)
            .OrderBy(g => g.Title)
            .ToListAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its unique identifier.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to retrieve.</param>
    /// <returns>
    /// A <see cref="Task{Game}"/> representing the asynchronous operation, with a result of the matching <see cref="Game"/> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method uses Entity Framework's <c>FindAsync</c> to efficiently locate a game by its primary key.
    /// It first checks the context's local cache before querying the database.
    /// </remarks>
    public async Task<Game?> GetByIdAsync(int gameId, bool trackChanges = false)
    {

        //return await context.Game.FindAsync(gameId);
        return await FindByCondition(g => g.Id == gameId, trackChanges)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a <see cref="Game"/> entity by its title, using a case-insensitive comparison.
    /// </summary>
    /// <param name="gameTitle">The title of the game to retrieve.</param>
    /// <returns>
    /// A <see cref="Task{Game}"/> representing the asynchronous operation, with a result of the first <see cref="Game"/> that matches the given title; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method performs a case-insensitive search using <see cref="string.Equals(string, string, StringComparison)"/> with <c>OrdinalIgnoreCase</c>.
    /// </remarks>
    public async Task<Game?> GetByTitleAsync(string gameTitle)
    {
        //Game? game = await context.Game.FirstOrDefaultAsync(g => g.Title.ToLower() == gameTitle.ToLower());
        Game? game = await FindByCondition(g => g.Title.Equals(gameTitle))
            .FirstAsync();
        return game;
    }

    /// <summary>
    /// Removes the specified game from the data store.
    /// </summary>
    /// <remarks>This method removes the provided game instance from the underlying data store. Ensure that
    /// the  game exists in the data store before calling this method to avoid unexpected behavior.</remarks>
    /// <param name="game">The game to be removed. Cannot be null.</param>
    public void Remove(Game game)
    {
        //context.Game.Remove(game);
        Delete(game);
    }

    /// <summary>
    /// Updates the specified game entity in the database.
    /// </summary>
    /// <remarks>This method modifies the existing game record in the database to reflect the changes in the
    /// provided <paramref name="game"/> object. Ensure that the <paramref name="game"/> object contains valid and
    /// updated data before calling this method.</remarks>
    /// <param name="game">The game entity to update. Cannot be null.</param>
    public void Update(Game game)
    {
        //context.Game.Update(game);
        //context.Entry(game).State = EntityState.Modified;
        Update(game);
    }
    /// <summary>
    /// Asynchronously retrieves all games associated with a specific tournament.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament whose games are to be retrieved.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of
    /// <see cref="Game"/> objects related to the specified tournament. If no games are found, the collection will be empty.
    /// </returns>
    public async Task<IEnumerable<Game?>> GetByTournamentIdAsync(int tournamentId, bool trackChanges = false)
    {
        //return await context.Game
        //    .Where(g => g.TournamentDetailsId == tournamentId)
        //    .ToListAsync();
        return await FindByCondition(g => g.TournamentDetailsId == tournamentId, trackChanges)
            .OrderBy(g => g.Title)
            .ToListAsync();
    }
}
