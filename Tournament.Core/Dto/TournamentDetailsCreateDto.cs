// Ignore Spelling: Dto

namespace Tournament.Core.Dto
{
    /// <summary>
    /// Data Transfer Object used to create a new tournament.
    /// </summary>
    public class TournamentDetailsCreateDto
    {
        /// <summary>
        /// Gets or sets the title of the tournament.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the start date of the tournament.
        /// </summary>
        public required DateTime StartDate { get; set; }
    }
}
