// Ignore Spelling: Dto

// -----------------------------------------------------------------------------
// File: TournamentDto.cs
// Summary: Data Transfer Object (DTO) representing tournament details, including
//          title, start date, calculated end date, and a list of associated games.
// <author> [Clive Leddy] </author>
// <created> [2025-06-27] </created>
// Notes: Used for transferring tournament data with an auto-calculated EndDate
//        and nested GameDto list for related games.
// -----------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Tournament.Core.Dto;
/// <summary>
/// Represents a data transfer object (DTO) for encapsulating tournament information,
/// including its title, start date, computed end date, and a collection of associated games.
/// </summary>
/// <remarks>
/// This class is used primarily for read operations where tournament data is exposed to clients.
/// The <see cref="EndDate"/> property is automatically derived by adding three months to the
/// <see cref="StartDate"/>, eliminating the need to persist it in storage.
///
/// Key characteristics:
/// - Title: The name of the tournament.
/// - StartDate: The date on which the tournament begins.
/// - EndDate: Computed as StartDate + 3 months, not manually settable.
/// - Games: A list of <see cref="GameDto"/> instances representing games linked to the tournament.
///
/// This DTO facilitates data projection and serialization for API responses or service interactions.
/// </remarks>
public class TournamentDto
{
    /// <summary>
    /// Gets or sets the title of the tournament.
    /// </summary>
    [Required(ErrorMessage = "Title is a required field.")]
    [StringLength(100, ErrorMessage = "Maximum length for the Title is 100 characters.")]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the start date of the tournament.
    /// </summary>
    [Required(ErrorMessage = "StartDate is a required field.")]
    public required DateTime StartDate { get; set; }


    /// <summary>
    /// Gets the end date of the tournament, automatically calculated as StartDate plus 3 months.
    /// <note>EndDate is automatically calculated as StartDate + 3 months and always stays in sync with StartDate.</note>
    /// </summary>
    public DateTime EndDate => StartDate.AddMonths(3);


    /// <summary>
    /// Navigation Property.
    /// Gets or sets the list of games associated with the tournament.
    /// </summary>
    [Required(ErrorMessage = "Games collection is required.")]
    public required List<GameDto> Games { get; set; }
}
