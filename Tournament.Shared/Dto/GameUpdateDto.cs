// Ignore Spelling: Dto

// ------------------------------------------------------------------------------------
// File: GameUpdateDto.cs
// Summary: Data Transfer Object (DTO) for updating an existing Game entity,
//          including validation attributes for modifiable fields.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Contains only the properties allowed to be updated, enforcing data integrity
//        via required and length constraints.
// ------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Tournaments.Shared.Dto
{
    public record GameUpdateDto
    {
        /// <summary>
        /// Gets or sets the title of the game.
        /// This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required(ErrorMessage = "Title is a required field.")]
        [MaxLength(100, ErrorMessage = "Maximum length for the Title is 100 characters.")]
        public required string Title { get; set; }
    }
}
