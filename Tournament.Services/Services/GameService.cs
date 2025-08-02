// Ignore Spelling: Dto

// -----------------------------------------------------------------------------
// File: GameService.cs
// Summary: Implements business-layer operations for “Game” resources – including
//          creation, retrieval, update, patch, and deletion – while enforcing
//          domain rules (e.g., max-game limit) and returning rich
//          <see cref="Domain.Models.Responses.ApiBaseResponse"/> results.
// Author: [Clive Leddy]
// Created: [2025-07-09]
// Notes:  Uses AutoMapper for DTO↔entity mapping and the Unit-of-Work pattern
//         for persistence. All public members are fully XML-documented for
//         Swagger / IntelliSense.
// -----------------------------------------------------------------------------

using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Responses;
using Service.Contracts;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Tournaments.Services.Services;

/// <summary>
/// Service layer responsible for all game-related actions (CRUD, patch, existence
/// checks, pagination, and business validations).  
/// Depends on an <see cref="IUnitOfWork"/> for data access and an
/// <see cref="IMapper"/> for DTO mapping, returning standardized
/// <see cref="ApiBaseResponse"/> objects so controllers can delegate
/// error-handling uniformly.
/// </summary>
/// <remarks>
/// The class acts as a facade, hiding repository complexity from controllers by providing 
/// a simple interface and enforcing cross-entity rules such as <see cref="MaxNumberOfGames"/>.
/// </remarks>
public class GameService(IMapper mapper, IUnitOfWork unitOfWork) : IGameService
{

    /// <summary>
    /// Maximum number of games allowed inside a single tournament.
    /// Requests that would exceed this number yield an
    /// <see cref="ApiMaxGameLimitReachedResponse"/>.
    /// </summary>
    public int MaxNumberOfGames { get; init; } = 10;

    /// <summary>
    /// Retrieves a paginated list of games associated with a specific tournament,
    /// applying any request parameters such as page number and size. If the page number
    /// exceeds the available total pages, it automatically adjusts the page and retries the query.
    /// </summary>
    /// <param name="requestParameters">Pagination and filtering parameters for the query.</param>
    /// <param name="tournamentId">The unique ID of the tournament to retrieve games from.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><see cref="ApiBaseResponse"/>: An <see cref="ApiOkResponse{T}"/> with the game data if successful, or an <see cref="ApiTournamentNotFoundResponse"/> if no games are found.</item>
    /// <item><see cref="MetaData"/>: Pagination metadata including current page, page size, total items, and total pages.</item>
    /// </list>
    /// </returns>
    public async Task<(ApiBaseResponse gameResponse, MetaData metaData)> GetGamesAsync(TournamentRequestParameters requestParameters, int tournamentId)
    {
        TournamentRequestParameters clampedParameters =  ServiceManager.ClampRequestParameters(requestParameters);

        PagedList<Game> pagedList = await unitOfWork
            .GameRepository
            .GetByTournamentIdAsync(clampedParameters, tournamentId);

        // Clamp PageNumber if it exceeds total pages
        if(pagedList.MetaData.TotalPages > 0 && clampedParameters.PageNumber > pagedList.MetaData.TotalPages) {
            clampedParameters.PageNumber = pagedList.MetaData.TotalPages;

            // Fetch again with corrected PageNumber
            pagedList = await unitOfWork
                .GameRepository
                .GetByTournamentIdAsync(clampedParameters, tournamentId);
        }

        IEnumerable<GameDto> gameDtos = mapper.Map<IEnumerable<GameDto>>(pagedList.Items);

        if(!gameDtos.Any()) {
            return (new ApiTournamentNotFoundResponse($"Tournament with id {tournamentId} not found."), pagedList.MetaData);
        }

        return (new ApiOkResponse<IEnumerable<GameDto>>(gameDtos), pagedList.MetaData);
    }

    /// <summary>
    /// Retrieves a game by its unique identifier within the specified tournament.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament that the game is expected to belong to.</param>
    /// <param name="id">The unique identifier of the game to retrieve.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> that contains either:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> with the game data if found and associated with the given tournament.</item>
    /// <item><see cref="ApiGameNotFoundByIdResponse"/> if no matching game is found or it does not belong to the specified tournament.</item>
    /// </list>
    /// </returns>
    public async Task<ApiBaseResponse> GetGameAsync(int tournamentId, int id)
    {
        Game? gameExists = await unitOfWork.GameRepository.GetByIdAsync(id);

        if(gameExists is null || gameExists.TournamentDetailsId != tournamentId) {
            return new ApiGameNotFoundByIdResponse($"Game with id {id} was not found.");
        }

        // Map the retrieved game entity to a GameDto object using AutoMapper.
        GameDto gameDto = mapper.Map<GameDto>(gameExists);

        return new ApiOkResponse<GameDto>(gameDto);
    }

    /// <summary>
    /// Retrieves a game by its title within the specified tournament.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament to search within.</param>
    /// <param name="title">The title of the game to retrieve.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> that contains either:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> with the game data if found.</item>
    /// <item><see cref="ApiGameNotFoundByTitleResponse"/> if no game with the given title exists in the tournament.</item>
    /// </list>
    /// </returns>
    public async Task<ApiBaseResponse> GetGameAsync(int tournamentId, string title)
    {
        Game? gameExists = await unitOfWork.GameRepository.GetByTitleAndTournamentIdAsync(title, tournamentId);
        // Check if a game with the same title already exists for the specified tournament.
        if(gameExists is null) {
            return new ApiGameNotFoundByTitleResponse($"Game with title {title} was not found.");
        }

        return new ApiOkResponse<GameDto>(mapper.Map<GameDto>(gameExists));
    }

    /// <summary>
    /// Removes a game from the specified tournament by its ID.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament from which the game should be removed.</param>
    /// <param name="id">The ID of the game to remove.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> indicating the result of the removal operation. Possible responses include:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> if the game was successfully removed.</item>
    /// <item><see cref="ApiNotFoundResponse"/> if the game does not exist or does not belong to the specified tournament.</item>
    /// </list>
    /// </returns>
    public async Task<ApiBaseResponse> RemoveAsync(int tournamentId, int id)
    {
        Game? game = await unitOfWork.GameRepository.GetByIdAsync(id);

        if(game == null) {

            return new ApiNotFoundResponse($"Could not remove the game from tournament {tournamentId}.");
        }
        // Check if the game exists and is part of the specified tournament.
        if(game.TournamentDetailsId != tournamentId) {

            return new ApiNotFoundResponse($"Could not remove game {game.Title} from tournament.");
        }

        unitOfWork.GameRepository.Remove(game);
        await unitOfWork.CompleteAsync();

        return new ApiOkResponse<GameDto>(mapper.Map<GameDto>(game));
    }

    /// <summary>
    /// Checks if a game with the specified ID exists in the repository.
    /// </summary>
    /// <param name="id">The ID of the game to check for existence.</param>
    /// <returns>
    /// An <see cref="ApiOkResponse{bool}"/> with value true if the game exists; 
    /// otherwise, an <see cref="ApiNotFoundResponse"/> indicating the game was not found.
    /// </returns>
    public async Task<ApiBaseResponse> ExistsAsync(int id)
    {
        bool entityExists = await unitOfWork.GameRepository.AnyAsync(id);
        return entityExists ? new ApiOkResponse<bool>(entityExists) : new ApiNotFoundResponse("Game does not exists.");
    }

    /// <summary>
    /// Adds a new game to the specified tournament if it doesn't already exist and the game limit hasn't been exceeded.
    /// </summary>
    /// <param name="gameCreateDto">The data transfer object containing the game details to be created.</param>
    /// <param name="tournamentId">The ID of the tournament to which the game should be added.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> representing the result of the add operation. Possible responses include:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> if the game was successfully added.</item>
    /// <item><see cref="ApiAlreadyExistsResponse"/> if a game with the same name already exists in the tournament.</item>
    /// <item><see cref="ApiMaxGameLimitReachedResponse"/> if the tournament has reached the maximum number of games allowed.</item>
    /// <item><see cref="ApiSaveFailedResponse"/> if the game could not be saved to the database.</item>
    /// </list>
    /// </returns>
    public async Task<ApiBaseResponse> AddGameAsync(GameCreateDto gameCreateDto, int tournamentId)
    {
        //Validation of GameCreateDto with tournamentId
        // Check if there is a duplicate game in the tournament.
        Game? existingGameEntity = await unitOfWork.GameRepository.GetByTitleAndTournamentIdAsync(gameCreateDto.Name, tournamentId);

        if(existingGameEntity is not null) {
            // Duplicate game found, return a failure result with the existing game.
            return new ApiAlreadyExistsResponse($"A game with the name '{gameCreateDto.Name}' already exists in tournament ID {tournamentId}.");
        }

        // Use your existing paginated method with a high page size to fetch count
        var requestParams = new TournamentRequestParameters
        {
            PageNumber = 1,
            PageSize = MaxNumberOfGames + 1 // To check if limit is already exceeded
        };

        PagedList<Game> games = await unitOfWork.GameRepository.GetByTournamentIdAsync(requestParams,tournamentId);

        if(games.Items.Count >= MaxNumberOfGames) {
            return new ApiMaxGameLimitReachedResponse($"Tournament {tournamentId} has reached its maximum number of {MaxNumberOfGames} games per tournament.");
        }

        // Map the GameCreateDto to a Game entity.
        Game gameEntity = mapper.Map<Game>(gameCreateDto);
        // Associate the game with the specified tournament.
        gameEntity.TournamentDetailsId = tournamentId;

        // Persist the changes to the database.
        unitOfWork.GameRepository.Add(gameEntity);
        int changes = await unitOfWork.CompleteAsync();

        // Check if any changes were made to the database.
        if(changes == 0) {
            // If no changes were made, return a failure result.
            return new ApiSaveFailedResponse("Failed to save the new game.");
        }
        return new ApiOkResponse<GameDto>(mapper.Map<GameDto>(gameEntity));
    }

    /// <summary>
    /// Updates an existing game in a tournament using the provided title and update data.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament to which the game belongs.</param>
    /// <param name="title">The title of the game to be updated.</param>
    /// <param name="gameUpdateDto">The DTO containing updated properties for the game.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> representing the result of the update operation. Possible responses include:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> if the game is successfully updated.</item>
    /// <item><see cref="ApiGameNotFoundByTitleResponse"/> if the game is not found by title.</item>
    /// <item><see cref="ApiNoChangesMadeResponse"/> if no changes were applied during update.</item>
    /// </list>
    /// </returns>
    public async Task<ApiBaseResponse> UpdateAsync(int tournamentId, string title, GameUpdateDto gameUpdateDto)
    {
        // Retrieve the game by title and tournament ID.
        Game? gameEntity = await unitOfWork.GameRepository.GetByTitleAndTournamentIdAsync(title, tournamentId);

        // If the game does not exist, return null.
        if(gameEntity == null) {
            //return UpdateGameResult.NotFound;
            return new ApiGameNotFoundByTitleResponse($"Could not find game {title} in tournament {tournamentId}.");
        }
        // Map the updated properties from the DTO to the existing game entity.
        mapper.Map(gameUpdateDto, gameEntity);

        // UpdateAsync the game in the repository.
        unitOfWork.GameRepository.Update(gameEntity);
        int success = await unitOfWork.CompleteAsync();

        return success != 0 ? new ApiOkResponse<GameDto>(mapper.Map<GameDto>(gameEntity)) : new ApiNoChangesMadeResponse($"The game {title} was not updated.");

    }

    /// <summary>
    /// Applies updates from a <see cref="GameDto"/> to an existing game entity identified by its ID,
    /// validating that the game's start date falls within the specified tournament date range.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament the game belongs to.</param>
    /// <param name="id">The ID of the game to be updated.</param>
    /// <param name="gameDto">The DTO containing the updated game data.</param>
    /// <param name="tournamentDto">The DTO containing the tournament's date range for validation.</param>
    /// <returns>
    /// An <see cref="ApiBaseResponse"/> representing the result of the operation. This may be:
    /// <list type="bullet">
    /// <item><see cref="ApiOkResponse{T}"/> if the update is successful.</item>
    /// <item><see cref="ApiGameNotFoundByIdResponse"/> if the game could not be found.</item>
    /// <item><see cref="ApiUnProcessableContentResponse"/> if the date is out of range.</item>
    /// <item><see cref="ApiNotModifiedResponse"/> if no changes were made.</item>
    /// </list>
    /// </returns>
    public async Task<ApiBaseResponse> ApplyToAsync(int tournamentId, int id, GameDto gameDto, TournamentDto tournamentDto)
    {

        if(gameDto.StartDate < tournamentDto.StartDate || gameDto.StartDate > tournamentDto.EndDate) {
            //return ApplyPatchResult.InvalidDateRange;
            return new ApiUnProcessableContentResponse("Date is out of range for the tournament.");
        }

        // Fetch the existing game by ID.
        Game? gameEntity = await unitOfWork.GameRepository.GetByIdAsync(id);
        if(gameEntity is null) {
            //return ApplyPatchResult.GameNotFound;
            return new ApiGameNotFoundByIdResponse($"Game with id {id} was not found.");
        }

        // Map gameEntity to a GameDto.
        mapper.Map(gameDto, gameEntity);

        // UpdateAsync the existing game details in the repository.
        unitOfWork.GameRepository.Update(gameEntity);

        // Persist the changes to the database.
        int result = await unitOfWork.CompleteAsync();
        if(result == 0) {
            //return ApplyPatchResult.NoChanges;
            return new ApiNotModifiedResponse("The game entity was not updated.");
        }

        // Fetch fresh data from DB to reflect all updates
        Game? updateEntity = await unitOfWork.GameRepository.GetByIdAsync(id, trackChanges:false);
        GameDto? updateDto = mapper.Map<GameDto>(updateEntity);

        return new ApiOkResponse<GameDto>(updateDto);
    }
}