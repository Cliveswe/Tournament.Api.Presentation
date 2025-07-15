// Ignore Spelling: Api

namespace Domain.Models.Responses;
public abstract class ApiBaseResponse(bool success, string? message = null)
{
    public bool Success { get; init; } = success;
    //Support general feedback across all responses (not just NotFound).
    public string? Message { get; init; } = message;

    //Helper method 
    public bool IsNotFound() => this is ApiNotFoundResponse;


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

public abstract class ApiNotFoundResponse(string message) : ApiBaseResponse(false, message)
{
}

public class TournamentNotFoundResponse(int id)
    : ApiNotFoundResponse($"Tournament with id {id} not found.")
{
}

public class GameNotFoundByTitleResponse(string title) :
    ApiNotFoundResponse($"Game with title {title} was not found.")
{

}

public class GameNotFoundByIdResponse(int id) :
    ApiNotFoundResponse($"Game with id {id} was not found.")
{

}

public class GameDoesNotBelongToTournamentResponse(int gameId, int tournamentId) :
    ApiNotFoundResponse($"Game with ID {gameId} does not belong to Tournament ID {tournamentId}.")
{
}
