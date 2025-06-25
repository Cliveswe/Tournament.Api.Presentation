// Ignore Spelling: Uo

using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
/// <summary>
/// Provides a unit of work implementation for managing repositories and saving changes to the data store.
/// </summary>
/// <remarks>This class encapsulates the repositories for accessing and managing tournament and game-related data,
/// and provides a method to save changes to the underlying data store. It is designed to ensure that operations on the
/// data context are performed in a consistent and transactional manner.</remarks>
/// <param name="context"></param>
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
