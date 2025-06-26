using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;

/// <summary>
/// Defines repository methods for managing <see cref="TournamentDetails"/> entities.
/// Provides asynchronous operations for retrieving, adding, updating, removing,
/// and checking the existence of tournament details.
/// </summary>
public interface ITournamentDetailsRepository
{
    /// <summary>
    /// Asynchronously retrieves all tournament details.
    /// </summary>
    /// <param name="includeGames">If true, includes related game entities in the results.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a collection of <see cref="TournamentDetails"/>.</returns>
    Task<IEnumerable<TournamentDetails>> GetAllAsync(bool includeGames = false);

    /// <summary>
    /// Asynchronously retrieves a tournament detail by its unique identifier.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament to retrieve.</param>
    /// <param name="includeGames">If true, includes related game entities in the result.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the <see cref="TournamentDetails"/> if found; otherwise, <c>null</c>.</returns>
    Task<TournamentDetails?> GetAsync(int tournamentId, bool includeGames = false);

    /// <summary>
    /// Checks asynchronously whether a tournament detail exists with the specified ID.
    /// </summary>
    /// <param name="id">The tournament detail identifier to check for existence.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains <c>true</c> if a tournament with the specified ID exists; otherwise, <c>false</c>.</returns>
    Task<bool> AnyAsync(int id);

    /// <summary>
    /// Adds a new tournament detail to the repository.
    /// </summary>
    /// <param name="tournament">The tournament detail entity to add.</param>
    void Add(TournamentDetails tournament);

    /// <summary>
    /// Updates an existing tournament detail in the repository.
    /// </summary>
    /// <param name="tournament">The tournament detail entity with updated values.</param>
    void Update(TournamentDetails tournament);

    /// <summary>
    /// Removes the specified tournament detail from the repository.
    /// </summary>
    /// <param name="tournament">The tournament detail entity to remove.</param>
    void Remove(TournamentDetails tournament);

    /// <summary>
    /// Checks asynchronously whether a tournament exists with the specified title and start date.
    /// </summary>
    /// <param name="title">The title of the tournament.</param>
    /// <param name="startDate">The start date of the tournament.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains <c>true</c> if such a tournament exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByTitleAndStartDateAsync(string title, DateTime startDate);
}
