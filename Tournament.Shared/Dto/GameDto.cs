// Ignore Spelling: Dto

// -----------------------------------------------------------------------------
// File: GameDto.cs
// Summary: Data Transfer Object (DTO) representing game details,
//          including the game title and scheduled start date.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Used to transfer game information between application layers.
// -----------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Tournaments.Shared.Dto;

/// <summary>
/// Data Transfer Object (DTO) representing the essential details of a game,
/// including its title and scheduled start date.
/// </summary>
/// <remarks>
/// This DTO is used primarily for transferring game information between application layers,
/// facilitating serialization and deserialization in API requests and responses.
///
/// Key properties:
/// - <see cref="Title"/>: The name of the game.
/// - <see cref="StartDate"/>: The scheduled date and time when the game begins.
///
/// This class promotes separation of concerns by decoupling the domain entity from data transport,
/// supporting clean architecture and maintainability.
/// </remarks>

public record GameDto
{
    /// <summary>
    /// Gets or sets the title of the game.
    /// </summary>
    [Required(ErrorMessage = "Game title is required.")]
    [MaxLength(100, ErrorMessage = "Maximum length for the Title is 100 characters.")]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the scheduled start date and time of the game.
    /// </summary>
    [Required(ErrorMessage = "Time is required.")]
    public required DateTime StartDate { get; set; }
}
