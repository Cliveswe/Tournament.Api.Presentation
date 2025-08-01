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
/// Represents a Data Transfer Object (DTO) used to update the title of an existing tournament.
/// </summary>
/// <remarks>
/// This DTO is intended strictly for update (PUT/PATCH) operations where only mutable fields of a 
/// tournament can be changed. It ensures that:
/// 
/// <list type="bullet">
///   <item>
///     <term><see cref="Title"/></term>
///     <description>
///     Must be provided and limited to a maximum of 100 characters. Used to rename a tournament.
///     </description>
///   </item>
/// </list>
/// 
/// Immutable fields like identifiers or associated collections (e.g., games) are intentionally excluded
/// to protect domain integrity and simplify validation.
/// 
/// Validation attributes ensure that the update request conforms to business rules before reaching 
/// the application or data layer.
/// </remarks>
public record TournamentUpdateDto
{
    /// <summary>
    /// Gets or sets the title of the tournament.
    /// This field is required and cannot exceed 100 characters.
    /// </summary>
    [Required(ErrorMessage = "Title is a required field.")]
    [MaxLength(100, ErrorMessage = "Maximum length for the Title is 100 characters.")]
    public required string Title { get; set; }
}
