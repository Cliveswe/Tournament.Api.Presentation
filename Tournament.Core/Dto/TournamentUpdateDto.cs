using System.ComponentModel.DataAnnotations;

namespace Tournament.Core.Dto;

/// <summary>
/// Data Transfer Object for updating an existing tournament.
/// Includes only the properties that are permitted to be changed by the client.
/// </summary>
public class TournamentUpdateDto
{
    /// <summary>
    /// Gets or sets the title of the tournament.
    /// This field is required and cannot exceed 100 characters.
    /// </summary>
    [Required(ErrorMessage = "Title is a required field.")]
    [MaxLength(100, ErrorMessage = "Maximum length for the Title is 100 characters.")]
    public required string Title { get; set; }
}
