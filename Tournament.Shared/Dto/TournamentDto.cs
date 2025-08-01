// Ignore Spelling: Dto

// --------------------------------------------------------------------------------
// File: TournamentDto.cs
// Summary: Data Transfer Object (DTO) representing tournament details, including
//          title, start date, calculated end date, and a list of associated games.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Used for transferring tournament data with an auto-calculated EndDate
//        and nested GameDto list for related games.
// --------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tournaments.Shared.Dto;

/// <summary>
/// Data Transfer Object (DTO) representing the details of a tournament, 
/// including its name, start date, computed end date, and associated games.
/// </summary>
/// <remarks>
/// This DTO is intended for read operations (e.g., API responses) and supports structured serialization 
/// of tournament-related data.
/// 
/// <para>
/// The <see cref="EndDate"/> property is computed automatically based on the <see cref="StartDate"/>,
/// extending it by 3 months. It is not meant to be manually set or persisted.
/// </para>
/// 
/// <list type="bullet">
///   <item>
///     <term><see cref="Title"/></term>
///     <description>Required. The tournament's display name (max 100 characters).</description>
///   </item>
///   <item>
///     <term><see cref="StartDate"/></term>
///     <description>Required. The tournament's official start date.</description>
///   </item>
///   <item>
///     <term><see cref="EndDate"/></term>
///     <description>Derived from <see cref="StartDate"/>. Equals StartDate + 3 months.</description>
///   </item>
///   <item>
///     <term><see cref="Games"/></term>
///     <description>Required. A list of games (of type <see cref="GameDto"/>) associated with the tournament.</description>
///   </item>
/// </list>
/// </remarks>
public record TournamentDto
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


    /// <summary>
    /// Gets the end date of the tournament, automatically calculated as StartDate plus 3 months.
    /// <note>EndDate is automatically calculated as StartDate + 3 months and always stays in sync with StartDate.</note>
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateTime EndDate => StartDate.AddMonths(3);


    /// <summary>
    /// Navigation Property.
    /// Gets or sets the list of games associated with the tournament.
    /// </summary>
    [Required(ErrorMessage = "Games collection is required.")]
    [JsonPropertyName("games")]
    public required List<GameDto> Games { get; set; }
}
