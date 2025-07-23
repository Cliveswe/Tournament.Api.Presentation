// Ignore Spelling: Api Timestamp
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
    public bool IsBadGamePatchDocumentResponse() => this is BadGamePatchDocumentResponse;
    public bool IsGameNotFoundByIdResponse() => this is GameNotFoundByIdResponse;
    public bool NotModifiedResponse() => this is NotModifiedResponse;
    public bool UnProcessableContentResponse() => this is UnProcessableContentResponse;
    public bool NoChangesMadeResponse() => this is NoChangesMadeResponse;

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

/// <summary>
/// 404 Not Found status code.
/// </summary>
/// <param name="message">Display error message.</param>
public class ApiNotFoundResponse(string message) : ApiBaseResponse(false, message, StatusCodes.Status404NotFound)
{
}

public class ApiConflictResponse(string message) : ApiBaseResponse(false, message, StatusCodes.Status409Conflict);
#region Custom Api Responses


public class TournamentNotFoundResponse(string message)
    : ApiNotFoundResponse(message)
{
}

public class GameNotFoundByTitleResponse(string message)
    : ApiNotFoundResponse(message)
{
}

public class GameNotFoundByIdResponse(string message)
    : ApiNotFoundResponse(message)
{
}

public class BadRequestResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status400BadRequest)
{ }

/// <summary>
/// 
/// </summary>
/// <param name="message"></param>
public class GameAlreadyExistsResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status409Conflict)
{
}

public class GameSaveFailedResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status500InternalServerError)
{
}

public class MaxGameLimitReachedResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status409Conflict)
{ }

public class BadGamePatchDocumentResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status400BadRequest)
{ }

public class NotModifiedResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status304NotModified)
{ }

public class UnProcessableContentResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status422UnprocessableEntity)
{ }

public class NoChangesMadeResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status422UnprocessableEntity)
{ }
#endregion