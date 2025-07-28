// Ignore Spelling: Dto

using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Responses;
using Service.Contracts;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Tournaments.Services.Services;
public class GameService(IMapper mapper, IUnitOfWork uoW) : IGameService
{
    public int MaxNumberOfGames { get; init; } = 10;

    public async Task<(ApiBaseResponse gameResponse, MetaData metaData)> GetGamesAsync(TournamentRequestParameters requestParameters, int tournamentId)
    {
        TournamentRequestParameters clampedParameters =  ServiceManager.ClampRequestParameters(requestParameters);

        PagedList<Game> pagedList = await uoW
            .GameRepository
            .GetByTournamentIdAsync(clampedParameters, tournamentId);

        // Clamp PageNumber if it exceeds total pages
        if(pagedList.MetaData.TotalPages > 0 && clampedParameters.PageNumber > pagedList.MetaData.TotalPages) {
            clampedParameters.PageNumber = pagedList.MetaData.TotalPages;

            // Fetch again with corrected PageNumber
            pagedList = await uoW
                .GameRepository
                .GetByTournamentIdAsync(clampedParameters, tournamentId);
        }

        IEnumerable<GameDto> gameDtos = mapper.Map<IEnumerable<GameDto>>(pagedList.Items);

        if(!gameDtos.Any()) {
            return (new TournamentNotFoundResponse($"Tournament with id {tournamentId} not found."), pagedList.MetaData);
        }

        return (new ApiOkResponse<IEnumerable<GameDto>>(gameDtos), pagedList.MetaData);
    }

    public async Task<ApiBaseResponse> GetGameAsync(int tournamentId, int id)
    {
        Game? gameExists = await uoW.GameRepository.GetByIdAsync(id);

        if(gameExists is null || gameExists.TournamentDetailsId != tournamentId) {
            return new GameNotFoundByIdResponse($"Game with id {id} was not found.");
        }

        // Map the retrieved game entity to a GameDto object using AutoMapper.
        GameDto gameDto = mapper.Map<GameDto>(gameExists);

        return new ApiOkResponse<GameDto>(gameDto);
    }

    public async Task<ApiBaseResponse> GetGameAsync(int tournamentId, string title)
    {
        Game? gameExists = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(title, tournamentId);
        // Check if a game with the same title already exists for the specified tournament.
        if(gameExists is null) {
            return new GameNotFoundByTitleResponse($"Game with title {title} was not found.");
        }

        return new ApiOkResponse<GameDto>(mapper.Map<GameDto>(gameExists));
    }

    //public async Task<bool> RemoveAsync(int tournamentId, int id)
    public async Task<ApiBaseResponse> RemoveAsync(int tournamentId, int id)
    {
        Game? game = await uoW.GameRepository.GetByIdAsync(id);

        if(game == null) {

            return new ApiNotFoundResponse($"Could not remove the game from tournament {tournamentId}.");
        }
        // Check if the game exists and is part of the specified tournament.
        if(game.TournamentDetailsId != tournamentId) {

            return new ApiNotFoundResponse($"Could not remove game {game.Title} from tournament.");
        }

        uoW.GameRepository.Remove(game);
        await uoW.CompleteAsync();

        return new ApiOkResponse<GameDto>(mapper.Map<GameDto>(game));
    }
    public async Task<ApiBaseResponse> ExistsAsync(int id)
    {
        bool entityExists = await uoW.GameRepository.AnyAsync(id);
        return entityExists ? new ApiOkResponse<bool>(entityExists) : new ApiNotFoundResponse("Game does not exists.");
    }

    public async Task<ApiBaseResponse> AddGameAsync(GameCreateDto gameCreateDto, int tournamentId)
    {
        //Validation of GameCreateDto with tournamentId
        // Check if there is a duplicate game in the tournament.
        Game? existingGameEntity = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(gameCreateDto.Name, tournamentId);

        if(existingGameEntity is not null) {
            // Duplicate game found, return a failure result with the existing game.
            return new GameAlreadyExistsResponse($"A game with the name '{gameCreateDto.Name}' already exists in tournament ID {tournamentId}.");
        }

        // Use your existing paginated method with a high page size to fetch count
        var requestParams = new TournamentRequestParameters
        {
            PageNumber = 1,
            PageSize = MaxNumberOfGames + 1 // To check if limit is already exceeded
        };

        PagedList<Game> games = await uoW.GameRepository.GetByTournamentIdAsync(requestParams,tournamentId);

        if(games.Items.Count >= MaxNumberOfGames) {
            return new MaxGameLimitReachedResponse($"Tournament {tournamentId} has reached its maximum number of {MaxNumberOfGames} games per tournament.");
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
            return new GameSaveFailedResponse("Failed to save the new game.");
        }
        return new ApiOkResponse<GameDto>(mapper.Map<GameDto>(gameEntity));
    }

    public async Task<ApiBaseResponse> UpdateAsync(int tournamentId, string title, GameUpdateDto gameUpdateDto)
    {
        // Retrieve the game by title and tournament ID.
        Game? gameEntity = await uoW.GameRepository.GetByTitleAndTournamentIdAsync(title, tournamentId);

        // If the game does not exist, return null.
        if(gameEntity == null) {
            //return UpdateGameResult.NotFound;
            return new GameNotFoundByTitleResponse($"Could not find game {title} in tournament {tournamentId}.");
        }
        // Map the updated properties from the DTO to the existing game entity.
        mapper.Map(gameUpdateDto, gameEntity);

        // Update the game in the repository.
        uoW.GameRepository.Update(gameEntity);
        int success = await uoW.CompleteAsync();

        return success != 0 ? new ApiOkResponse<GameDto>(mapper.Map<GameDto>(gameEntity)) : new NoChangesMadeResponse($"The game {title} was not updated.");

    }

    public async Task<ApiBaseResponse> ApplyToAsync(int tournamentId, int id, GameDto gameDto, TournamentDto tournamentDto)
    {

        if(gameDto.StartDate < tournamentDto.StartDate || gameDto.StartDate > tournamentDto.EndDate) {
            //return ApplyPatchResult.InvalidDateRange;
            return new UnProcessableContentResponse("Date is out of range for the tournament.");
        }

        // Fetch the existing game by ID.
        Game? gameEntity = await uoW.GameRepository.GetByIdAsync(id);
        if(gameEntity is null) {
            //return ApplyPatchResult.GameNotFound;
            return new GameNotFoundByIdResponse($"Game with id {id} was not found.");
        }

        // Map gameEntity to a GameDto.
        mapper.Map(gameDto, gameEntity);

        // Update the existing game details in the repository.
        uoW.GameRepository.Update(gameEntity);

        // Persist the changes to the database.
        int result = await uoW.CompleteAsync();
        if(result == 0) {
            //return ApplyPatchResult.NoChanges;
            return new NotModifiedResponse("The game entity was not updated.");
        }

        // Fetch fresh data from DB to reflect all updates
        Game? updateEntity = await uoW.GameRepository.GetByIdAsync(id, trackChanges:false);
        GameDto? updateDto = mapper.Map<GameDto>(updateEntity);

        return new ApiOkResponse<GameDto>(updateDto);
    }
}