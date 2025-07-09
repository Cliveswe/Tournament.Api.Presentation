// Ignore Spelling: Dto

// ------------------------------------------------------------------------------------
// File: GameCreateDto.cs
// Summary: Data Transfer Object (DTO) for creating a new Game entity,
//          including validation attributes to ensure required fields and constraints.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Used to encapsulate input data for creating a game,
//        enforcing validation on title, scheduled time, and tournament association.
// ------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Tournament.Shared.Dto
{
    /// <summary>
    /// Data Transfer Object (DTO) used for creating a new <see cref="Game"/> entity.
    /// Encapsulates the required data for game creation, including the game’s title,
    /// scheduled time, and associated tournament identifier, with validation rules
    /// to enforce input correctness and integrity.
    ///
    /// Key features:
    /// - Validates that the title is provided and has a length between 3 and 100 characters.
    /// - Requires a scheduled time to indicate when the game will occur.
    /// - Ensures the tournament identifier is specified and positive.
    ///
    /// This DTO is primarily used to receive input data from clients during game creation operations.
    /// </summary>
    public record GameCreateDto
    {
        /// <summary>
        /// Gets or sets the name (title) of the game.
        /// This field is required and must be between 3 and 100 characters in length.
        /// </summary>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the scheduled time of the game.
        /// This field is required and indicates when the game is planned to start.
        /// </summary>
        [Required(ErrorMessage = "Time is required.")]
        public required DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the tournament to which this game belongs.
        /// This field is required and must be a positive integer greater than zero.
        /// </summary>
        //[Required(ErrorMessage = "TournamentDetailsId is required.")]
        //[Range(1, int.MaxValue, ErrorMessage = "TournamentDetailsId must be greater than 0.")]
        //public required int TournamentDetailsId { get; set; }
    }
}
