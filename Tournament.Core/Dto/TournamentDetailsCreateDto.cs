// Ignore Spelling: Dto
// -----------------------------------------------------------------------------
// File: TournamentDetailsCreateDto.cs
// Summary: Defines a data transfer object used for creating new tournaments.
//          Includes validation attributes to ensure required fields and constraints.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Utilizes data annotations for input validation on title and start date.
// -----------------------------------------------------------------------------


using System.ComponentModel.DataAnnotations;

namespace Tournament.Core.Dto
{
    /// <summary>
    /// Represents a Data Transfer Object (DTO) for creating new tournament entries.
    /// </summary>
    /// <remarks>
    /// This DTO is used in create operations where new tournament data is submitted by the client.
    /// It includes validation rules to ensure required information is present and meets expected formats.
    ///
    /// Key properties:
    /// - <see cref="Title"/>: Required, limited to 100 characters.
    /// - <see cref="StartDate"/>: Required, represents when the tournament begins.
    ///
    /// The validation attributes applied to each property support automatic model binding
    /// and server-side validation in web API scenarios.
    /// </remarks>
    public class TournamentDetailsCreateDto
    {
        /// <summary>
        /// Gets or sets the title of the tournament.
        /// </summary>
        [Required(ErrorMessage = "Title is a required field.")]
        [StringLength(100, ErrorMessage = "Maximum length for the Title is 100 characters.")]
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the start date of the tournament.
        /// </summary>
        [Required(ErrorMessage = "StartDate is a required field.")]
        public required DateTime StartDate { get; set; }
    }
}
