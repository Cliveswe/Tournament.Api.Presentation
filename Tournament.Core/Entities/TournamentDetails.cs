// --------------------------------------------------------------------------------------------
// File: TournamentDetails.cs
// Summary: Defines the TournamentDetails entity representing a tournament's core data,
//          including its unique ID, title, start date, and related games.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: This class models the tournament entity and includes navigation to associated games.
// --------------------------------------------------------------------------------------------

namespace Tournament.Core.Entities
{
    /// <summary>
    /// Represents a tournament entity within the domain model, capturing essential information
    /// such as its unique identifier, title, start date, and related games.
    /// </summary>
    /// <remarks>
    /// This entity serves as the aggregate root for tournament-related data, modeling the core
    /// attributes and relationships necessary to manage a tournament's lifecycle and structure.
    /// 
    /// Key properties include:
    /// - <see cref="Id"/>: The primary key uniquely identifying the tournament.
    /// - <see cref="Title"/>: The name or title of the tournament.
    /// - <see cref="StartDate"/>: The scheduled start date of the tournament.
    /// - <see cref="Games"/>: A navigation property representing the collection of associated games.
    /// 
    /// Used across data access, business logic, and presentation layers to encapsulate and persist
    /// tournament-specific data. Designed for use with Entity Framework Core and supports
    /// relational navigation for efficient data querying.
    /// </remarks>
    public class TournamentDetails
    {
        /// <summary>
        /// Gets or sets the unique identifier for the tournament.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title or name of the tournament.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the start date of the tournament.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the collection of games associated with the tournament.
        /// Navigation property representing all games in this tournament.
        /// </summary>
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
