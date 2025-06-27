// Ignore Spelling: Dto

// -----------------------------------------------------------------------------
// File: GameDto.cs
// Summary: Data Transfer Object (DTO) representing game details,
//          including the game title and scheduled start date.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Used to transfer game information between application layers.
// -----------------------------------------------------------------------------

namespace Tournament.Core.Dto;

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

public class GameDto
{
    /// <summary>
    /// Gets or sets the title of the game.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the scheduled start date and time of the game.
    /// </summary>
    public DateTime StartDate { get; set; }
}
