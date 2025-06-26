namespace Tournament.Core.Entities
{
    /// <summary>
    /// Represents the details of a tournament, including its title, start date, and associated games.
    /// </summary>
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
