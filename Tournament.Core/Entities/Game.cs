namespace Tournament.Core.Entities
{
    /// <summary>
    /// Represents a game that is part of a tournament, including its title, scheduled time, 
    /// and navigation to the associated tournament.
    /// </summary>
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
