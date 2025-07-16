// Ignore Spelling: Api
using Microsoft.AspNetCore.Http;  // for StatusCodes

namespace Domain.Models.Responses;
public abstract class ApiBaseResponse(bool success, string? message = null, int statusCode = 200)
{
    public bool Success { get; init; } = success;
    //Support general feedback across all responses (not just NotFound).
    public string? Message { get; init; } = message;
    public int StatusCode { get; init; } = statusCode;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;


    //Helper methods
    public bool IsNotFound() => this is ApiNotFoundResponse;
    public bool IsMaxGameLimitReached() => this is MaxGameLimitReachedResponse;


    public TResultType GetOkResult<TResultType>()
    {
        if(this is ApiOkResponse<TResultType> apiOkResponse) {
            return apiOkResponse.Result;
        }
        throw new InvalidOperationException($"Expected ApiOkResponse<{typeof(TResultType).Name}>, but received {this.GetType().Name}.");
    }

}

public sealed class ApiOkResponse<TResult>(TResult result, string? message = null) : ApiBaseResponse(true, message)
{
    public TResult Result { get; } = result;
}

public class ApiNotFoundResponse(string message) : ApiBaseResponse(false, message, StatusCodes.Status404NotFound)
{
}

#region Custom Api Responses


public class TournamentNotFoundResponse(int id)
    : ApiNotFoundResponse($"Tournament with id {id} not found.")
{
}

public class GameNotFoundByTitleResponse(string title)
    : ApiNotFoundResponse($"Game with title {title} was not found.")
{
}

public class GameNotFoundByIdResponse(int id)
    : ApiNotFoundResponse($"Game with id {id} was not found.")
{
}


public class GameAlreadyExistsResponse(string name, int tournamentId)
    : ApiBaseResponse(false, $"A game with the name '{name}' already exists in tournament ID {tournamentId}.", StatusCodes.Status409Conflict)
{
}

public class GameSaveFailedResponse()
    : ApiBaseResponse(false, "Failed to save the new game.", StatusCodes.Status500InternalServerError)
{
}

public class MaxGameLimitReachedResponse(int maxGamesPerTournament, int tournamentId)
    : ApiBaseResponse(false, $"Tournament {tournamentId} has reached its maximum number of {maxGamesPerTournament} games per tournament.", StatusCodes.Status409Conflict)
{
}


//public class TournamentNotFoundResponse(int id)
//    : ApiBaseResponse(false, $"Tournament with id {id} not found.")
//{
//}

//public class GameNotFoundByTitleResponse(string title) :
//    ApiBaseResponse(false, $"Game with title {title} was not found.")
//{

//}

//public class GameNotFoundByIdResponse(int id) :
//    ApiBaseResponse(false, $"Game with id {id} was not found.")
//{

//}

//public class GameAlreadyExistsResponse(string name, int tournamentId)
//    : ApiBaseResponse(false, $"A game with the name '{name}' already exists in tournament ID {tournamentId}.")
//{
//}

//public class GameSaveFailedResponse() : ApiBaseResponse(false, "Failed to save the new game.")
//{
//}

//public class MaxGameLimitReachedResponse(int maxGamesPerTournament, int tournamentId)
//    : ApiBaseResponse(false, $"Tournament {tournamentId} has reached its maximum number of {maxGamesPerTournament} games per tournament.")
//{ }
#endregion