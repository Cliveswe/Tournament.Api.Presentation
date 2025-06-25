// Ignore Spelling: Dto

namespace Tournament.Core.Dto;
public class TournamentDto
{
    public string Title { get; set; }
    public DateTime StartDate { get; set; }

    // EndDate is automatically calculated as StartDate + 3 months and always
    //stays in sync with StartDate.
    public DateTime EndDate => StartDate.AddMonths(3);

    // Add this property to map the games
    public List<GameDto> Games { get; set; }
}
