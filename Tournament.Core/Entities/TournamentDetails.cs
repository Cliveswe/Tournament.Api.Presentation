namespace Tournament.Core.Entities;
public class TournamentDetails
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public ICollection<Game> Games { get; set; }
}
