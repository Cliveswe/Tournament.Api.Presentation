// Ignore Spelling: Api

namespace Domain.Models.Responses;
public abstract class ApiBaseResponse(bool success)
{
    public bool Success { get; set; } = success;

    public TResultType GetOkResult<TResultType>()
    {
        if(this is ApiOkResponse<TResultType> apiOkResponse) {
            return apiOkResponse.Result;
        }
        throw new InvalidOperationException($"Response type {this.GetType().Name} is not an ApiOkResponse.");
    }

}

public sealed class ApiOkResponse<TResult>(TResult result) : ApiBaseResponse(true)
{
    public TResult Result { get; } = result;
}

public abstract class ApiNotFoundResponse(string message) : ApiBaseResponse(false)
{
    public string? Message { get; } = message;
}

public class TournamentNotFoundResponse(int id)
    : ApiNotFoundResponse($"Tournament with id {id} not found.")
{
}