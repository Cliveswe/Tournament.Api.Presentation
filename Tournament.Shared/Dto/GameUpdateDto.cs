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
using System.Text.Json.Serialization;

namespace Tournaments.Shared.Dto;

/// <summary>
/// Data Transfer Object (DTO) used for updating an existing game's title.
/// </summary>
/// <remarks>
/// This DTO is designed to carry only the mutable property of a game—its <see cref="Title"/>.
/// It is used primarily in update operations to encapsulate client input,
/// promoting separation of concerns between domain and transport layers.
/// 
/// Validation:
/// - <see cref="Title"/> is required and must not exceed 100 characters.
/// </remarks>
public record GameUpdateDto
{
    /// <summary>
    /// Gets or sets the title of the game.
    /// This field is required and has a maximum length of 100 characters.
    /// </summary>
    [Required(ErrorMessage = "Title is a required field.")]
    [MaxLength(100, ErrorMessage = "Maximum length for the Title is 100 characters.")]
    [JsonPropertyName("title")]
    public required string Title { get; set; }
}
