// Ignore Spelling: Dto

using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Responses;
using Service.Contracts;
using Service.Contracts.Enums;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Tournaments.Services.Services;
public class GameService(IMapper mapper, IUnitOfWork uoW) : IGameService
{


    //public async Task<IEnumerable<GameDto>> GetAllAsync(int tournamentId)
    //public async Task<(IEnumerable<GameDto> gameDtos, MetaData metaData)> GetAllAsync(TournamentRequestParameters requestParameters, int tournamentId)
    //{
    //    // Retrieve all games associated with the specified tournament ID.
    //    // var gamesResult = await uoW.GameRepository.GetByTournamentIdAsync(tournamentId);
    //    // Map the retrieved game entities to GameDto objects using AutoMapper.
    //    //IEnumerable<GameDto> games = mapper.Map<IEnumerable<GameDto>>(gamesResult);

    //    var pagedList = await uoW
    //        .GameRepository
    //        .GetByTournamentIdAsync(requestParameters, tournamentId);
    //    var gameDtos = mapper.Map<IEnumerable<GameDto>>(pagedList.Items);

    //    return (gameDtos, pagedList.MetaData);
    //}

    public async Task<(ApiBaseResponse gameResponse, MetaData metaData)> GetGamesAsync(TournamentRequestParameters requestParameters, int tournamentId)
    {
        // Retrieve all games associated with the specified tournament ID.
        // var gamesResult = await uoW.GameRepository.GetByTournamentIdAsync(tournamentId);
        // Map the retrieved game entities to GameDto objects using AutoMapper.
        //IEnumerable<GameDto> games = mapper.Map<IEnumerable<GameDto>>(gamesResult);

        var pagedList = await uoW
            .GameRepository
            .GetByTournamentIdAsync(requestParameters, tournamentId);
        IEnumerable<GameDto> gameDtos = mapper.Map<IEnumerable<GameDto>>(pagedList.Items);

        if(!gameDtos.Any()) {
            return (new TournamentNotFoundResponse(tournamentId), pagedList.MetaData);
        }

        return (new ApiOkResponse<IEnumerable<GameDto>>(gameDtos), pagedList.MetaData);
    }


    public async Task<ApiBaseResponse> GetGameAsync(int tournamentId, int id)
    {
        //
        Game? gameExists = await uoW.GameRepository.GetByIdAsync(id);

        //
        if(gameExists is null || gameExists.TournamentDetailsId != tournamentId) {
            return new GameNotFoundByIdResponse(id);
        }
        // Map the retrieved game entity to a GameDto object using AutoMapper.
        GameDto gameDto = mapper.Map<GameDto>(gameExists);

        return new ApiOkResponse<GameDto>(gameDto);
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

    public async Task<ApiBaseResponse> GetGameAsync(int tournamentId, string title)
    {
        Game? gameExists = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(title, tournamentId);
        // Check if a game with the same title already exists for the specified tournament.
        if(gameExists is null) {
            return new GameNotFoundByTitleResponse(title);
        }

        return new ApiOkResponse<GameDto>(mapper.Map<GameDto>(gameExists));
    }

    //public async Task<GameDto> GetAsync(int tournamentId, string title)
    //{

    //    Game? game = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(title, tournamentId);
    //    // Check if a game with the same title already exists for the specified tournament.
    //    if(game == null) {
    //        return mapper.Map<GameDto>(game);
    //    }

    //    return mapper.Map<GameDto>(game);
    //}

    public async Task<bool> RemoveAsync(int tournamentId, int id)
    {
        Game? game = await uoW.GameRepository.GetByIdAsync(id);

        // Check if the game exists and is part of the specified tournament.
        if(game!.TournamentDetailsId != tournamentId) {
            return false;
        }

        uoW.GameRepository.Remove(game);
        await uoW.CompleteAsync();
        return true;
    }
    public async Task<bool> ExistsAsync(int id)
    {
        return await uoW.GameRepository.AnyAsync(id);
    }

    public async Task<ApiBaseResponse> AddAsync(GameCreateDto gameCreateDto, int tournamentId)
    {
        //Validation of GameCreateDto with tournamentId
        // Check if there is a duplicate game in the tournament.
        Game? existingGameEntity = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(gameCreateDto.Name, tournamentId);

        if(existingGameEntity is not null) {
            // Duplicate game found, return a failure result with the existing game.
            return new GameAlreadyExistsResponse(gameCreateDto.Name, tournamentId);
        }

        // Map the GameCreateDto to a Game entity.
        Game gameEntity = mapper.Map<Game>(gameCreateDto);
        // Associate the game with the specified tournament.
        gameEntity.TournamentDetailsId = tournamentId;

        // Persist the changes to the database.
        uoW.GameRepository.Add(gameEntity);
        int changes = await uoW.CompleteAsync();

        // Check if any changes were made to the database.
        if(changes == 0) {
            // If no changes were made, return a failure result.
            return new GameSaveFailedResponse();
        }
        return new ApiOkResponse<GameDto>(mapper.Map<GameDto>(gameEntity));
    }

    public async Task<UpdateGameResult> UpdateAsync(int tournamentId, string title, GameUpdateDto gameUpdateDto)
    {
        // Retrieve the game by title and tournament ID.
        Game? gameEntity = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(title, tournamentId);

        // If the game does not exist, return null.
        if(gameEntity == null) {
            return UpdateGameResult.NotFound;
        }
        // Map the updated properties from the DTO to the existing game entity.
        mapper.Map(gameUpdateDto, gameEntity);

        // Update the game in the repository.
        uoW.GameRepository.Update(gameEntity);
        int success = await uoW.CompleteAsync();

        if(success == 0) {
            // If no changes were made, return NotModified.
            return UpdateGameResult.NotModified;
        }
        // Return the updated game as a GameDto.
        return UpdateGameResult.Success;
    }

    public async Task<ApplyPatchResult> ApplyToAsync(int tournamentId, int id, GameDto gameDto, TournamentDto tournamentDto)
    {

        if(gameDto.StartDate < tournamentDto.StartDate || gameDto.StartDate > tournamentDto.EndDate) {
            return ApplyPatchResult.InvalidDateRange;
        }

        // Fetch the existing game by ID.
        Game? gameEntity = await uoW.GameRepository.GetByIdAsync(id);
        if(gameEntity is null) {
            return ApplyPatchResult.GameNotFound;
        }

        // Map gameEntity to a GameDto.
        mapper.Map(gameDto, gameEntity);

        // Update the existing game details in the repository.
        uoW.GameRepository.Update(gameEntity);

        // Persist the changes to the database.
        int result = await uoW.CompleteAsync();
        if(result == 0) {
            return ApplyPatchResult.NoChanges;
        }

        return ApplyPatchResult.Success;
    }
}