// Ignore Spelling: Uo

// -----------------------------------------------------------------------------
// File: UoW.cs
// Summary: Implements the Unit of Work pattern for managing tournament and game repositories,
//          coordinating data operations, and committing changes to the data store.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Encapsulates repository instances and provides a method to save changes asynchronously
//        to ensure transactional consistency in data operations.
// -----------------------------------------------------------------------------

using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
/// <summary>
/// Implements the Unit of Work (UoW) pattern to coordinate operations across multiple repositories
/// using a shared database context. This class centralizes repository access and ensures that
/// all changes are committed as a single, atomic transaction.
/// </summary>
/// <remarks>
/// Responsibilities:
/// - Provides controlled access to <see cref="ITournamentDetailsRepository"/> and <see cref="IGameRepository"/> instances.
/// - Manages the lifecycle of the <see cref="TournamentApiContext"/> to track changes and unify operations.
/// - Facilitates asynchronous saving of changes, preserving consistency and enabling rollback on failure.
///
/// This implementation promotes separation of concerns, improves testability, and simplifies transaction handling
/// within the data access layer.
/// </remarks>
public class UoW(TournamentApiContext context) : IUoW
{
    /// <summary>
    /// Gets the repository for accessing and managing tournament details.
    /// </summary>
    public ITournamentDetailsRepository TournamentDetailsRepository => new TournamentDetailsRepository(context);

    /// <summary>
    /// Gets the repository for managing game-related data operations.
    /// </summary>
    public IGameRepository GameRepository => new GameRepository(context);

    /// <summary>
    /// Saves all pending changes to the underlying data store asynchronously.
    /// </summary>
    /// <remarks>This method commits any modifications made to the tracked entities in the current context.
    /// Ensure that all changes are valid before calling this method, as it will persist them to the data
    /// store.</remarks>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }
}
