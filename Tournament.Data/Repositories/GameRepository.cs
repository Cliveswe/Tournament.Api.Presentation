using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;

/// <summary>
/// Provides methods for managing game entities in the database.
/// </summary>
/// <remarks>The <see cref="GameRepository"/> class serves as a repository for performing CRUD operations on game
/// entities. It interacts with the underlying database context to add, retrieve, update, and remove game records. This
/// class is designed to abstract the data access layer, providing a consistent interface for managing game
/// data.</remarks>
/// <param name="context"></param>
public class GameRepository(TournamentApiContext context) : IGameRepository
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
        context.Game.Add(game);
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
        return await context.Game.AnyAsync(g => g.Id == id);
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

    public async Task<bool> ExistsByNameAndDateAsync(string name, DateTime date)
    {
        return await context.Game.AnyAsync(g => g.Title == name && g.Time == date);
    }

    /// <summary>
    /// Asynchronously retrieves all games from the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of  <see cref="Game"/>
    /// objects representing all games in the database.</returns>
    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await context.Game.ToListAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a game by its unique identifier.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to retrieve.</param>
    /// <returns>A <see cref="Game"/> object representing the game with the specified identifier,  or <see langword="null"/> if
    /// no game with the given identifier exists.</returns>
    public async Task<Game?> GetAsync(int gameId)
    {
        return await context.Game.FindAsync(gameId);
    }

    /// <summary>
    /// Removes the specified game from the data store.
    /// </summary>
    /// <remarks>This method removes the provided game instance from the underlying data store. Ensure that
    /// the  game exists in the data store before calling this method to avoid unexpected behavior.</remarks>
    /// <param name="game">The game to be removed. Cannot be null.</param>
    public void Remove(Game game)
    {
        context.Game.Remove(game);
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
        context.Entry(game).State = EntityState.Modified;
        //context.Game.Update(game);
    }
}
