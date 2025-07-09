using AutoMapper;
using Domain.Contracts;
using Service.Contracts;

namespace Tournaments.Services.Services;
public class GameService(IMapper mapper, IUnitOfWork uoW) : IGameService
{ }