namespace Domain.Models.Exceptions;

public abstract class NotFoundException(string message, string title = "Not found")
    : Exception(message)
{
    public string Title { get; } = title;
}

public class TournamentNotFoundException(int id)
    : NotFoundException($"Tournament with id {id} not found.")
{

}

public class GameNotFoundException(int id)
    : NotFoundException($"Game with id {id} not found.")
{
}