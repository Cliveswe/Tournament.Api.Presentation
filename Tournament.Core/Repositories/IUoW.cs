// Ignore Spelling: Uo

namespace Tournament.Core.Repositories;
public interface IUoW
{
    ITournamentRepository TournamentRepository { get; }
    IGameRepository GameRepository { get; }
    Task CompleteAsync();
}
