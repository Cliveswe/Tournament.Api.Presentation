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
using System.Text.Json.Serialization;

namespace Tournaments.Shared.Dto;

/// <summary>
/// Data Transfer Object (DTO) used for creating a new game.
/// Contains the essential information such as the game's name and scheduled time.
/// </summary>
public record GameCreateDto
{
    /// <summary>
    /// Gets or sets the name (title) of the game.
    /// This field is required and must be between 3 and 100 characters in length.
    /// </summary>
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the scheduled time of the game.
    /// This field is required and indicates when the game is planned to start.
    /// </summary>
    [Required(ErrorMessage = "Time is required.")]
    [JsonPropertyName("time")]
    public required DateTime Time { get; set; }

}
