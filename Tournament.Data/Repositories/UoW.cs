// Ignore Spelling: Uo

// -----------------------------------------------------------------------------
// File: UoW.cs
// Summary: Implements the Unit of Work (UoW) pattern to coordinate repository
//          operations and manage transactional data persistence in the Tournament API.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes:
// - Provides a centralized interface for accessing multiple repositories and committing
//   changes to the data store using a shared EF Core context.
// - Promotes clean architecture, testability, and maintainability by decoupling
//   the data access layer from business logic.
// - Utilizes asynchronous saving to maintain transactional consistency across repositories.
//
// Change History:
// ----------------------------------------------------------------------------------
// Date         | Author        | Description
// ----------------------------------------------------------------------------------
// 2025-06-27   | Clive Leddy   | Initial creation of UoW class with direct repository instantiation.
// 2025-07-03   | Clive Leddy   | ✅ Re-factored to use constructor-injected Lazy<T> dependencies
//                              |    for ITournamentDetailsRepository and IGameRepository.
//                              |    Improved performance and life-cycle control by deferring
//                              |    repository instantiation until first use.
//                              |    Aligned with DI best practices in ASP.NET Core.
// -----------------------------------------------------------------------------


using Domain.Contracts;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
/// <summary>
/// Implements the Unit of Work (UoW) pattern to coordinate operations across multiple repositories
/// using a shared database context. This version supports lazy loading of repositories to improve performance
/// and control instantiation timing.
/// </summary>
/// <remarks>
/// Responsibilities:
/// - Provides controlled access to <see cref="ITournamentDetailsRepository"/> and <see cref="IGameRepository"/> instances,
///   which are now injected as <see cref="Lazy{T}"/> to defer instantiation until first use.
/// - Manages the life-cycle of the <see cref="TournamentApiContext"/> to track entity state and unify operations across repositories.
/// - Facilitates asynchronous saving of changes using <see cref="CompleteAsync"/>, ensuring transactional consistency.
///
/// This updated implementation enhances performance and dependency management while continuing to promote
/// separation of concerns, testability, and maintainability within the data access layer.
/// </remarks>
public class UoW(TournamentApiContext context, Lazy<ITournamentDetailsRepository> tournamentDetailsRepository, Lazy<IGameRepository> gameRepository) : IUoW
{
    #region previous implementation
    //private readonly Lazy<ITournamentDetailsRepository> tournamentDetailsRepository;
    //private readonly Lazy<IGameRepository> gameRepository;

    /// <summary>
    /// Gets the repository for accessing and managing tournament details.
    /// </summary>
    //public ITournamentDetailsRepository TournamentDetailsRepository => new TournamentDetailsRepository(context);

    /// <summary>
    /// Gets the repository for managing game-related data operations.
    /// </summary>
    //public IGameRepository GameRepository => new GameRepository(context);

    #endregion
    /// <summary>
    /// Gets the repository for accessing and managing tournament details.
    /// </summary>
    /// <remarks>
    /// This property leverages lazy-loading to defer instantiation of the
    /// <see cref="ITournamentDetailsRepository"/> until it is first accessed. This improves performance
    /// and resource efficiency, especially in scoped dependency scenarios.
    /// </remarks>
    public ITournamentDetailsRepository TournamentDetailsRepository => tournamentDetailsRepository.Value;

    /// <summary>
    /// Gets the repository for managing game-related data operations.
    /// </summary>
    /// <remarks>
    /// The <see cref="IGameRepository"/> is injected as a <see cref="Lazy{T}"/> instance to optimize
    /// service resolution. This defers its construction until it is required, ensuring minimal overhead
    /// during application startup or when the repository is unused in a given operation.
    /// </remarks>
    public IGameRepository GameRepository => gameRepository.Value;

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
