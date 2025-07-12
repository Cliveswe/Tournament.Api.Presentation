using System.ComponentModel.DataAnnotations;

namespace Tournaments.Shared.Request;
public class RequestParameters
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(2, 20)]
    public int PageSize { get; set; } = 5;
}

public class TournamentRequestParameters : RequestParameters
{
    public bool IncludeGames { get; set; } = false;
}
