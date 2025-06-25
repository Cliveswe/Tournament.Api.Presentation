using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
/// <summary>
/// Provides methods for managing tournament details in the data store.
/// </summary>
/// <remarks>This repository offers functionality to add, update, remove, and query tournament details. It
/// supports asynchronous operations for retrieving tournament data and checking for the existence of specific
/// tournament details. Use this class to interact with the underlying data context for tournament-related
/// operations.</remarks>
/// <param name="context"></param>
public class TournamentDetailsRepository(TournamentApiContext context) : ITournamentDetailsRepository
{
    /// <summary>
    /// Adds a new tournament to the data store.
    /// </summary>
    /// <param name="tournament">The details of the tournament to add. Cannot be null.</param>
    public void Add(TournamentDetails tournament)
    {
        context.TournamentDetails.Add(tournament);

    }

    /// <summary>
    /// Determines whether any tournament details exist with the specified identifier.
    /// </summary>
    /// <remarks>This method performs an asynchronous query against the underlying data source to check  for
    /// the existence of a tournament detail with the given identifier.</remarks>
    /// <param name="id">The identifier of the tournament detail to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains  <see langword="true"/> if a
    /// tournament detail with the specified identifier exists;  otherwise, <see langword="false"/>.</returns>
    public Task<bool> AnyAsync(int id)
    {
        return context.TournamentDetails.AnyAsync(t => t.Id == id);
    }

    #region old code
    /// <summary>
    /// Asynchronously retrieves all tournament details, including associated games.
    /// </summary>
    /// <remarks>This method returns a collection of <see cref="TournamentDetails"/> objects,      with the
    /// <see cref="TournamentDetails.Games"/> property populated for each tournament.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains      an <see
    /// cref="IEnumerable{TournamentDetails}"/> of all tournaments and their associated games.</returns>
    //public async Task<IEnumerable<TournamentDetails>> GetAllAsync()
    //{
    //    return await context.TournamentDetails
    //        .Include(g => g.Games)
    //        .ToListAsync();
    //}
    #endregion

    public async Task<IEnumerable<TournamentDetails>> GetAllAsync(bool includeGames = false)
    {
        return includeGames
             ? await context.TournamentDetails
                 .Include(t => t.Games)
                 .ToListAsync()
             : await context.TournamentDetails
                 .ToListAsync();
    }

    /// <summary>
    /// Retrieves the details of a tournament by its unique identifier.
    /// </summary>
    /// <remarks>This method performs an asynchronous database query to locate the tournament details 
    /// associated with the specified <paramref name="tournamentId"/>. If no matching tournament  is found, the method
    /// returns <see langword="null"/>.</remarks>
    /// <param name="tournamentId">The unique identifier of the tournament to retrieve.</param>
    /// <returns>A <see cref="TournamentDetails"/> object containing the tournament's details if found;  otherwise, <see
    /// langword="null"/>.</returns>
    public async Task<TournamentDetails?> GetAsync(int tournamentId)
    {
        return await context.TournamentDetails.FindAsync(tournamentId);
    }

    /// <summary>
    /// Removes the specified tournament from the data store.
    /// </summary>
    /// <remarks>This method removes the provided tournament from the underlying data context.  Ensure that
    /// the tournament exists in the data store before calling this method.</remarks>
    /// <param name="tournament">The tournament to be removed. Cannot be null.</param>
    public void Remove(TournamentDetails tournament)
    {
        context.TournamentDetails.Remove(tournament);
    }

    /// <summary>
    /// Updates the specified tournament details in the data store.
    /// </summary>
    /// <remarks>This method modifies the existing tournament record in the underlying data store. Ensure that
    /// the provided <paramref name="tournament"/> object contains valid and updated information.</remarks>
    /// <param name="tournament">The tournament details to update. Cannot be null.</param>
    public void Update(TournamentDetails tournament)
    {
        context.Entry(tournament).State = EntityState.Modified;
        //context.TournamentDetails.Update(tournament);
    }
}
