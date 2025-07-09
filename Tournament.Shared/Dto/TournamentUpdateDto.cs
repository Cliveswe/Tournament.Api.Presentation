// Ignore Spelling: Dto

// -----------------------------------------------------------------------------
// File: TournamentUpdateDto.cs
// Summary: Defines the DTO used for updating tournament data, exposing only
//          fields that clients are allowed to modify.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Contains validation attributes to enforce required fields and length
//        constraints, promoting data integrity during update operations.
// -----------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace Tournaments.Shared.Dto;

/// <summary>
/// Represents a data transfer object (DTO) used to update existing tournament information.
/// </summary>
/// <remarks>
/// This DTO is designed specifically for update operations, exposing only the modifiable properties
/// of a tournament to ensure data integrity and prevent unauthorized changes to immutable fields
/// such as identifiers or related entities.
///
/// Validation constraints:
/// - <see cref="Title"/>: Required field with a maximum length of 100 characters.
///
/// This class is typically used in API operations where clients submit partial updates to an existing
/// tournament entity. It supports model validation through data annotations to enforce business rules
/// before the update is applied to the persistence layer.
/// </remarks>
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
