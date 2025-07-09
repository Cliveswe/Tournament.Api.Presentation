// Ignore Spelling: Uo

// -----------------------------------------------------------------------------
// File: IUoW.cs
// Summary: Defines the interface for the Unit of Work pattern, providing
// coordinated access to repository instances and transactional commits.
//          Recent updates include support for lazy loading of repositories
//          to improve performance and dependency injection flexibility.
// Author: [Clive Leddy]
// Created: [2025-06-27]
// Notes: Facilitates centralized repository handling and asynchronous
// persistence of changes. Interface remains unchanged, while the implementation
// was enhanced to use Lazy<T> for repository instantiation.
// -----------------------------------------------------------------------------

namespace Domain.Contracts;

/// <summary>
/// Represents the Unit of Work pattern interface for coordinating multiple repositories
/// and committing changes to the data store as a single transaction.
/// </summary>
/// <remarks>
/// This interface provides access to repository interfaces for managing tournament details
/// and games, and includes a method to save all pending changes asynchronously.
/// Implementations ensure transactional consistency and promote separation of concerns.
/// </remarks>
public interface IUoW
{

    /// <summary>
    /// Gets the repository responsible for managing tournament details.
    /// </summary>
    ITournamentDetailsRepository TournamentDetailsRepository { get; }

    /// <summary>
    /// Gets the repository responsible for managing game data operations.
    /// </summary>
    IGameRepository GameRepository { get; }

    /// <summary>
    /// Asynchronously saves all changes made in the current unit of work to the underlying data store.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous save operation.
    /// The task result contains the number of state entries written to the database.
    /// </returns>
    Task<int> CompleteAsync();
}
