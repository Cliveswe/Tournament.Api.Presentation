// Ignore Spelling: Dto

// -----------------------------------------------------------------------------------
// File: TournamentDetailsCreateDto.cs
// Summary: Defines a data transfer object used for creating new tournaments.
//          Includes validation attributes to ensure required fields and constraints.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Utilizes data annotations for input validation on title and start date.
// -----------------------------------------------------------------------------------


using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tournaments.Shared.Dto;

/// <summary>
/// Data Transfer Object (DTO) used for creating new tournament entries.
/// </summary>
/// <remarks>
/// This DTO is intended for use in create operations where a client submits data to register a new tournament.
/// 
/// It includes validation attributes to enforce required fields and value constraints:
/// <list type="bullet">
///   <item>
///     <term><see cref="Title"/></term>
///     <description>Required; maximum length of 100 characters.</description>
///   </item>
///   <item>
///     <term><see cref="StartDate"/></term>
///     <description>Required; represents the scheduled start date of the tournament.</description>
///   </item>
/// </list>
/// 
/// The model is suitable for serialization and model binding in ASP.NET Core Web APIs,
/// and aligns with clean architecture practices by decoupling domain logic from transport structures.
/// </remarks>
public record TournamentDetailsCreateDto
{
    /// <summary>
    /// Gets or sets the title of the tournament.
    /// </summary>
    [Required(ErrorMessage = "Title is a required field.")]
    [StringLength(100, ErrorMessage = "Maximum length for the Title is 100 characters.")]
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the start date of the tournament.
    /// </summary>
    [Required(ErrorMessage = "StartDate is a required field.")]
    [JsonPropertyName("startDate")]
    public required DateTime StartDate { get; set; }
}
