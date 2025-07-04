using Service.Contracts;

namespace Tournament.Services.Services;
public class ServiceManager(Lazy<ITournamentService> tournamentService, Lazy<IGameService> gameService, Lazy<IAuthService> authService) : IServiceManager
{

    ITournamentService TournamentService => tournamentService.Value;
    IGameService GameService => gameService.Value;
    IAuthService AuthService => authService.Value;

}
