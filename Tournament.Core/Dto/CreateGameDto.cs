// Ignore Spelling: Dto

namespace Tournament.Core.Dto
{
    /// <summary>
    /// Data Transfer Object used to create a new game.
    /// </summary>
    public class GameCreateDto
    {
        /// <summary>
        /// Gets or sets the name of the game.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the scheduled time of the game.
        /// </summary>
        public required DateTime Time { get; set; }
    }
}
