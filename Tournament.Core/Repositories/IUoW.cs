// Ignore Spelling: Uo

namespace Tournament.Core.Repositories;
public interface IUoW
{
    ITournamentDetailsRepository TournamentDetailsRepository { get; }
    IGameRepository GameRepository { get; }
    Task<int> CompleteAsync();
}
