using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Service.Contracts;
using Tournaments.Shared.Dto;

namespace Tournaments.Services.Services;
public class GameService(IMapper mapper, IUnitOfWork uoW) : IGameService
{
    public async Task<IEnumerable<GameDto>> GetAllAsync(int tournamentId)
    {
        // Retrieve all games associated with the specified tournament ID.
        var gamesResult = await uoW.GameRepository.GetByTournamentIdAsync(tournamentId);
        // Map the retrieved game entities to GameDto objects using AutoMapper.
        IEnumerable<GameDto> games = mapper.Map<IEnumerable<GameDto>>(gamesResult);
        return games;
    }
    public async Task<GameDto?> GetAsync(int tournamentId, int id)
    {
        Game? game = await uoW.GameRepository.GetByIdAsync(id);

        // Check if the game exists and is part of the specified tournament.
        if(game == null || game.TournamentDetailsId != tournamentId) {
            return null;
        }
        // Map the retrieved game entity to a GameDto object using AutoMapper.
        GameDto gameDto = mapper.Map<GameDto>(game);

        return gameDto;
    }
    public async Task<GameDto> GetAsync(int tournamentId, string title)
    {

        Game? game = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(title, tournamentId);
        // Check if a game with the same title already exists for the specified tournament.
        if(game == null) {
            return mapper.Map<GameDto>(game);
        }

        return mapper.Map<GameDto>(game);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await uoW.GameRepository.AnyAsync(id);
    }

}