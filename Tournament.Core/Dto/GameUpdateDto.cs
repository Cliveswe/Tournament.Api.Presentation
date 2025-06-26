// Ignore Spelling: Dto
using System.ComponentModel.DataAnnotations;

namespace Tournament.Core.Dto
{
    /// <summary>
    /// Data Transfer Object (DTO) used to update an existing game.
    /// Contains only the fields that are allowed to be modified during an update operation.
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
