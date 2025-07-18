// Ignore Spelling: Api
using Microsoft.AspNetCore.Http;  // for StatusCodes

namespace Domain.Models.Responses;
public abstract class ApiBaseResponse(bool success, string? message = null, int statusCode = 200)
{
    public bool Success { get; init; } = success;
    //Support general feedback across all responses (not just NotFound).
    public string? Message { get; init; } = message;
    public int StatusCode { get; init; } = statusCode;
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;


    //Helper methods
    public bool IsNotFound() => this is ApiNotFoundResponse;
    public bool IsMaxGameLimitReached() => this is MaxGameLimitReachedResponse;
    public bool IsBadGamePatchDocumentResponse() => this is BadGamePatchDocumentResponse;
    public bool IsGameNotFoundByIdResponse() => this is GameNotFoundByIdResponse;
    public bool NotModifiedResponse() => this is NotModifiedResponse;
    public bool UnProcessableContentResponse() => this is UnProcessableContentResponse;


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


public class BadGamePatchDocumentResponse : ApiBaseResponse
{
    public IEnumerable<string> Errors { get; }

    public BadGamePatchDocumentResponse(
        string? message = "Patch document validation failed.",
        IEnumerable<string>? errors = null)
        : base(false, message, StatusCodes.Status400BadRequest)
    {
        Errors = errors ?? Array.Empty<string>();
    }
}


//public class BadGamePatchDocumentResponse : ApiBaseResponse
//{
//    public IEnumerable<string> Errors { get; }

//    public BadGamePatchDocumentResponse(IEnumerable<string> errors)
//        : base(false, "Patch document validation failed.", StatusCodes.Status400BadRequest)
//    {
//        Errors = errors;
//    }

//    public BadGamePatchDocumentResponse(string message)
//        : base(false, message, StatusCodes.Status400BadRequest)
//    {
//        Errors = Array.Empty<string>();
//    }
//}


public class NotModifiedResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status304NotModified)
{ }

public class UnProcessableContentResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status422UnprocessableEntity)
{ }
#endregion