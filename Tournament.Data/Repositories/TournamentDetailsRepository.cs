// ------------------------------------------------------------------------------------------------
// File: TournamentDetailsRepository.cs
// Summary: Implements repository methods for managing tournament details within the data store.
//          Provides functionality for adding, updating, removing, and querying tournaments,
//          including optional loading of related games. Supports asynchronous operations to ensure
//          efficient database access and transactional consistency.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Uses Entity Framework Core for data access. Includes methods for existence checks,
//        retrieval by ID, and filtering by title and start date.
// ------------------------------------------------------------------------------------------------
using Domain.Contracts;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Tournaments.Infrastructure.Data;
using Tournaments.Shared.Request;

namespace Tournaments.Infrastructure.Repositories;
/// <summary>
/// Provides a robust repository implementation for managing <see cref="TournamentDetails"/> entities within the data persistence layer.
/// </summary>
/// <remarks>
/// This class serves as the data access gateway for tournament-related operations, encapsulating all interactions with the
/// <see cref="TournamentApiContext"/> to facilitate Create, Read, Update, and Delete (CRUD) functionality. It supports asynchronous
/// execution patterns to optimize database communication and system scalability.
///
/// The repository enables conditional eager loading of related <see cref="Game"/> entities to efficiently retrieve comprehensive
/// tournament data sets. It also incorporates validation methods to check for the existence of tournaments by identifier,
/// title, and start date, thereby enforcing data integrity and preventing duplication.
///
/// Designed following the Repository pattern, this implementation promotes separation of concerns, testability, and maintainability
/// of the data access layer within the application architecture.
/// </remarks>
/// <param name="context">The EF Core database context used for querying and persisting tournament data.</param>
//public class TournamentDetailsRepository(TournamentApiContext context) : RepositoryBase<TournamentDetails>(context), ITournamentDetailsRepository
public class TournamentDetailsRepository(TournamentApiContext context) : RepositoryBase<TournamentDetails>(context), ITournamentDetailsRepository
{

    /// <summary>
    /// Adds a new tournament to the data store.
    /// </summary>
    /// <param name="tournament">The details of the tournament to add. Cannot be null.</param>
    public void Add(TournamentDetails tournament)
    {
        //context.TournamentDetails.Add(tournament);
        Create(tournament);

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
        // return context.TournamentDetails.AnyAsync(t => t.Id == id);
        return FindByCondition(t => t.Id.Equals(id), false)
            .AnyAsync();
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




    //public async Task<IEnumerable<TournamentDetails>> GetAllAsync(TournamentRequestParameters requestParameters, bool trackChanges = false)
    public async Task<PagedList<TournamentDetails>> GetAllAsync(TournamentRequestParameters requestParameters, bool trackChanges = false)
    {
        // Retrieve all tournaments without tracking changes
        IQueryable<TournamentDetails> tournaments = FindAll(trackChanges);

        if(requestParameters.IncludeGames) {
            // If IncludeGames is true, include related games and order them by title.
            tournaments = tournaments
                .Include(t => t.Games);
        }

        PagedList<TournamentDetails> pagedList = await PagedList<TournamentDetails>.CreateAsync(
            tournaments,
            requestParameters.PageNumber,
            requestParameters.PageSize);

        OrderGames(requestParameters, tournaments);

        return pagedList;
    }

    private static void OrderGames(TournamentRequestParameters requestParameters, IQueryable<TournamentDetails> tournaments)
    {
        if(requestParameters.IncludeGames) {
            foreach(TournamentDetails tournament in tournaments) {
                // If IncludeGames is true, order the games by title.
                tournament.Games = tournament.Games.OrderBy(g => g.Title).ToList();
            }
        }
    }



    /// <summary>
    /// Retrieves the details of a tournament by its unique identifier, optionally including related games.
    /// </summary>
    /// <remarks>
    /// This method performs an asynchronous database query to locate the tournament details associated with the specified <paramref name="tournamentId"/>.
    /// If <paramref name="includeGames"/> is <see langword="true"/>, the related games are loaded and ordered by their title.
    /// If no matching tournament is found, the method returns <see langword="null"/>.
    /// </remarks>
    /// <param name="tournamentId">The unique identifier of the tournament to retrieve.</param>
    /// <param name="includeGames">If <see langword="true"/>, includes the related games ordered by their title; otherwise, games are not included.</param>
    /// <returns>
    /// A <see cref="TournamentDetails"/> object containing the tournament's details and optionally its ordered games if found; otherwise, <see langword="null"/>.
    /// </returns>
    public async Task<TournamentDetails?> GetAsync(int tournamentId, bool includeGames = false, bool trackChanges = false)
    {
        // return await context.TournamentDetails.FindAsync(tournamentId);
        // Use Include to load related games if necessary
        //return includeGames ? await context.TournamentDetails
        //    .Include(t => t.Games) // include related games
        //    .FirstOrDefaultAsync(t => t.Id == tournamentId) :
        //    await context.TournamentDetails.FindAsync(tournamentId);

        //IEnumerable<TournamentDetails?> tournament;
        TournamentDetails? tournament;
        // Check if we need to include games.
        if(includeGames) {
            // If includeGames is true, load the tournament with its games.
            //tournament = await context.TournamentDetails
            //    .Include(t => t.Games)
            //    .FirstOrDefaultAsync(t => t.Id == tournamentId);

            tournament = await FindByCondition(t => t.Id.Equals(tournamentId), trackChanges)
                .Include(t => t.Games)
                .FirstOrDefaultAsync();

            // If the tournament is found, order its games by title.
            if(tournament != null && tournament.Games?.Any() == true) {
                tournament.Games = [.. tournament
                    .Games
                    .OrderBy(g => g.Title)];
            }
        } else {
            // If includeGames is false, just load the tournament without games.
            //tournament = await context.TournamentDetails
            //   .FirstOrDefaultAsync(t => t.Id == tournamentId);
            tournament = await FindByCondition(t => t.Id.Equals(tournamentId), trackChanges)
                .FirstOrDefaultAsync();
        }
        // Return the found tournament or null if not found.

        return tournament;
    }

    /// <summary>
    /// Removes the specified tournament from the data store.
    /// </summary>
    /// <remarks>This method removes the provided tournament from the underlying data context.  Ensure that
    /// the tournament exists in the data store before calling this method.</remarks>
    /// <param name="tournament">The tournament to be removed. Cannot be null.</param>
    public void Remove(TournamentDetails tournament)
    {
        //context.TournamentDetails.Remove(tournament);
        Delete(tournament);
    }

    /// <summary>
    /// Updates the specified tournament details in the data store.
    /// </summary>
    /// <remarks>This method modifies the existing tournament record in the underlying data store. Ensure that
    /// the provided <paramref name="tournament"/> object contains valid and updated information.</remarks>
    /// <param name="tournament">The tournament details to update. Cannot be null.</param>
    public new void Update(TournamentDetails tournament)
    {
        //context.Entry(tournament).State = EntityState.Modified;
        //context.TournamentDetails.Update(tournament);
        base.Update(tournament);
    }

    /// <summary>
    /// Asynchronously checks if any tournament exists with the specified title and start date.
    /// </summary>
    /// <param name="title">The title of the tournament to check for.</param>
    /// <param name="startDate">The start date of the tournament to check for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains <c>true</c> if a tournament with the specified title (case-insensitive)
    /// and start date exists; otherwise, <c>false</c>.
    /// </returns>
    public Task<bool> ExistsByTitleAndStartDateAsync(string title, DateTime startDate)
    {

        // return FindByCondition(t => t.Id.Equals(id), false).AnyAsync();

        // using ToLower() (which can be translated), is acceptable, although slightly less efficient.
        //return FindByCondition(t => t.Title.ToLower().Equals(title.ToLower()) && t.StartDate.Date == startDate.Date, false).AnyAsync();
        return FindByCondition(t => EF.Functions.Like(t.Title, title), false).AnyAsync();

        //return context.TournamentDetails
        // .AnyAsync(t => t.Title.ToLower() == title.ToLower() && t.StartDate.Date == startDate.Date);
    }
}
