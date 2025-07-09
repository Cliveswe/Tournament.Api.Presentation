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

namespace Tournament.Shared.Dto
{
    /// <summary>
    /// Data Transfer Object (DTO) designed specifically for updating an existing <see cref="Game"/> entity.
    /// This class includes only the modifiable properties, enforcing validation constraints to ensure data integrity.
    /// 
    /// Key features:
    /// - Contains the <see cref="Title"/> property with required and length validation.
    /// - Supports partial updates by restricting modifiable fields to those intended for change.
    /// 
    /// This DTO facilitates safe and controlled update operations by exposing only allowed fields
    /// and preventing modification of immutable or sensitive properties.
    /// </summary>
    public class GameUpdateDto
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
