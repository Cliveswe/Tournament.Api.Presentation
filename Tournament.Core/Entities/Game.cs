// -----------------------------------------------------------------------------
// File: Game.cs
// Summary: Defines the Game entity representing an individual game within a tournament,
//          including its unique ID, title, scheduled time, and link to the parent tournament.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: This class models the game entity with a foreign key relationship to TournamentDetails.
// -----------------------------------------------------------------------------

namespace Tournament.Core.Entities
{
    /// <summary>
    /// Represents a game entity within a tournament, capturing essential details such as its
    /// unique identifier, title, scheduled time, and association with a parent tournament.
    /// </summary>
    /// <remarks>
    /// This entity models a single game as part of the tournament domain. It includes both scalar
    /// properties (e.g., <see cref="Title"/>, <see cref="Time"/>) and relational mappings to support
    /// EF Core navigation and foreign key constraints.
    /// 
    /// Key properties include:
    /// - <see cref="Id"/>: The primary key identifying the game.
    /// - <see cref="Title"/>: The name of the game.
    /// - <see cref="Time"/>: The scheduled time at which the game is expected to take place.
    /// - <see cref="TournamentDetailsId"/>: The foreign key linking the game to a specific tournament.
    /// - <see cref="TournamentDetails"/>: The navigation property referencing the associated tournament entity.
    /// 
    /// This class supports database operations, data transfer, and domain modeling. It plays a central role in
    /// managing tournament scheduling and organization.
    /// </remarks>
    public class Game
    {
        /// <summary>
        /// Gets or sets the unique identifier for the game.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title or name of the game.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the scheduled time for the game.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets the foreign key referencing the tournament to which this game belongs.
        /// </summary>
        public int TournamentDetailsId { get; set; }

        /// <summary>
        /// Navigation property to the related <see cref="TournamentDetails"/> entity.
        /// </summary>
        public TournamentDetails TournamentDetails { get; set; }
    }
}
