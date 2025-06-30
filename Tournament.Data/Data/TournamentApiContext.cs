// Ignore Spelling: Api

// --------------------------------------------------------------------------------------------------
// File: TournamentApiContext.cs
// Summary: Defines the Entity Framework Core database context for the Tournament application,
//          providing DbSet properties for TournamentDetails and Game entities,
//          and managing database access and configurations.
// <author> [Clive Leddy] </author>
// <created> [2025-06-30] </created>
// Notes: Inherits from DbContext and configured with dependency injection for database connectivity.
// --------------------------------------------------------------------------------------------------


using Microsoft.EntityFrameworkCore;

namespace Tournament.Data.Data
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the Tournament application,
    /// managing access to tournament details and game entities.
    /// </summary>
    /// <remarks>
    /// This context class inherits from <see cref="DbContext"/> and is configured with options
    /// to connect to the underlying database. It provides <see cref="DbSet{T}"/> properties
    /// for querying and saving instances of <see cref="Core.Entities.TournamentDetails"/>
    /// and <see cref="Core.Entities.Game"/>.
    ///
    /// The context supports EF Core features such as change tracking, migrations, and LINQ queries.
    /// It serves as the primary gateway to the database for the application’s data layer.
    /// </remarks>
    public class TournamentApiContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentApiContext"/> class using the specified options.
        /// </summary>
        /// <param name="options">The options to configure the context, including the database provider and connection string.</param>
        public TournamentApiContext(DbContextOptions<TournamentApiContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TournamentDetails}"/> representing tournament detail entities.
        /// </summary>
        public DbSet<Core.Entities.TournamentDetails> TournamentDetails { get; set; } = default!;

        /// <summary>
        /// Gets or sets the <see cref="DbSet{Game}"/> representing game entities.
        /// </summary>
        public DbSet<Core.Entities.Game> Game { get; set; } = default!;
    }
}
