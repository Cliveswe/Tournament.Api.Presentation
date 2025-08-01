
// -------------------------------------------------------------------------------------
// File: RequestParameters.cs
// Summary: Contains base and specialized request parameter classes for paging and filtering 
//          tournament data in API endpoints.
// <author>[Clive Leddy] </author>
// <created> [2025-07-21] </created>
// Notes: Ensures validation and consistency in API pagination queries with 
//        configurable page size limits and optional data inclusion flags.
// -------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Tournaments.Shared.Request;

/// <summary>
/// Represents the base class for paginated API request parameters,
/// providing control over page number and page size with validation and clamping.
/// </summary>
public class RequestParameters
{
    private int pageSize = 20;
    private const int MaxPageSize = 100;
    private const int MinPageSize = 2;

    /// <summary>
    /// Gets or sets the number of items per page.
    /// Automatically clamps values outside the allowed range (2–100) to the max page size.
    /// </summary>
    public int PageSize {
        get => pageSize;
        set {
            if(value < MinPageSize || value > MaxPageSize) {
                pageSize = MaxPageSize; // Default/fall-back page size
            } else {
                pageSize = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the current page number.
    /// Must be greater than or equal to 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;
}

/// <summary>
/// Represents additional tournament-specific request parameters,
/// including optional inclusion of related game data in the response.
/// Inherits pagination behavior from <see cref="RequestParameters"/>.
/// </summary>
public class TournamentRequestParameters : RequestParameters
{
    /// <summary>
    /// Gets or sets a value indicating whether to include related games in the response.
    /// Default is false.
    /// </summary>
    public bool IncludeGames { get; set; } = false;
}
