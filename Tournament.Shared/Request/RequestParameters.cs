using System.ComponentModel.DataAnnotations;

namespace Tournaments.Shared.Request;
public class RequestParameters
{
    private int pageSize = 20;
    private const int MaxPageSize = 100;
    private const int MinPageSize = 2;

    /// <summary>
    /// Set the min and max range for page size.
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
    /// Page number.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;
}

public class TournamentRequestParameters : RequestParameters
{
    /// <summary>
    /// Include games.
    /// </summary>
    public bool IncludeGames { get; set; } = false;
}
