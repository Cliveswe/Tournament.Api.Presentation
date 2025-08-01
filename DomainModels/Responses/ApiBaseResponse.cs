// Ignore Spelling: Api Timestamp

// -------------------------------------------------------------------------------------
// File: ApiBaseResponse.cs
// Summary: Defines a base class for structured API responses and a set of derived
//          response types for common HTTP outcomes such as OK, NotFound, Conflict,
//          BadRequest, etc. Includes domain-specific variants for tournaments and games.
// <author> [Clive Leddy] </author>
// <created> [2025-07-19] </created>
// Notes: Provides uniform response modeling with metadata (success, message, status code),
//        time-stamping, and type-based evaluation helpers (e.g., Is<TResponse>()).
// -------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;  // for StatusCodes

namespace Domain.Models.Responses;

/// <summary>
/// Represents the base structure for all API responses, including shared metadata like success state,
/// message, HTTP status code, and timestamp.
/// </summary>
/// <param name="success">Indicates whether the response represents a successful operation.</param>
/// <param name="message">An optional message providing additional details about the response.</param>
/// <param name="statusCode">The HTTP status code associated with the response (default is 200).</param>
public abstract class ApiBaseResponse(bool success, string? message = null, int statusCode = 200)
{
    public bool Success { get; init; } = success;
    //Support general feedback across all responses (not just NotFound).
    public string? Message { get; init; } = message;
    public int StatusCode { get; init; } = statusCode;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;


    //Helper methods for, as an example, testing
    //public bool IsNotFound() => this is ApiNotFoundResponse;
    //public bool IsMaxGameLimitReached() => this is ApiMaxGameLimitReachedResponse;
    //public bool IsBadGamePatchDocumentResponse() => this is ApiBadGamePatchDocumentResponse;
    //public bool IsGameNotFoundByIdResponse() => this is ApiGameNotFoundByIdResponse;
    //public bool IsNotModifiedResponse() => this is ApiNotModifiedResponse;
    //public bool IsUnProcessableContentResponse() => this is ApiUnProcessableContentResponse;
    //public bool IsNoChangesMadeResponse() => this is ApiNoChangesMadeResponse;
    public bool Is<TResponse>() where TResponse : ApiBaseResponse => this is TResponse;


    public TResultType GetOkResult<TResultType>()
    {
        if(this is ApiOkResponse<TResultType> apiOkResponse) {
            return apiOkResponse.Result;
        }
        throw new InvalidOperationException($"Expected ApiOkResponse<{typeof(TResultType).Name}>, but received {this.GetType().Name}.");
    }

}

/// <summary>
/// Represents a successful 200 OK response containing a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <param name="result">The result payload to include in the response.</param>
/// <param name="message">An optional success message.</param>
/// <typeparam name="TResult">The type of the result returned by the API.</typeparam>
public sealed class ApiOkResponse<TResult>(TResult result, string? message = null) : ApiBaseResponse(true, message)
{
    public TResult Result { get; } = result;
}

/// <summary>
/// Represents a 404 Not Found API response when a requested resource could not be located.
/// </summary>
/// <param name="message">A message describing the missing resource.</param>
public class ApiNotFoundResponse(string message) : ApiBaseResponse(false, message, StatusCodes.Status404NotFound)
{
}

/// <summary>
/// Represents a 409 Conflict response when the request could not be completed due to a resource conflict.
/// </summary>
/// <param name="message">A message describing the nature of the conflict.</param>
public class ApiConflictResponse(string message) : ApiBaseResponse(false, message, StatusCodes.Status409Conflict);

/// <summary>
/// Represents a 204 No Content response indicating a successful request with no response body.
/// </summary>
public class ApiNoContentResponse() : ApiBaseResponse(true, null, StatusCodes.Status204NoContent);

/// <summary>
/// Represents a 404 Not Found response when a tournament could not be found.
/// </summary>
/// <param name="message">A message describing the missing tournament.</param>
public class ApiTournamentNotFoundResponse(string message)
    : ApiNotFoundResponse(message)
{
}

/// <summary>
/// Represents a 404 Not Found response when a game could not be found by its title.
/// </summary>
/// <param name="message">A message describing the missing game.</param>
public class ApiGameNotFoundByTitleResponse(string message)
    : ApiNotFoundResponse(message)
{
}

/// <summary>
/// Represents a 404 Not Found response when a game could not be found by its ID.
/// </summary>
/// <param name="message">A message describing the missing game ID.</param>
public class ApiGameNotFoundByIdResponse(string message)
    : ApiNotFoundResponse(message)
{
}

/// <summary>
/// Represents a 400 Bad Request response due to malformed syntax or validation errors.
/// </summary>
/// <param name="message">A message describing the bad request.</param>
public class ApiBadRequestResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status400BadRequest)
{ }

/// <summary>
/// Represents a 409 Conflict response indicating that the resource already exists.
/// </summary>
/// <param name="message">A message describing the conflict.</param>
public class ApiAlreadyExistsResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status409Conflict)
{
}

/// <summary>
/// Represents a 500 Internal Server Error response indicating a failure to save data.
/// </summary>
/// <param name="message">A message describing the failure.</param>
public class ApiSaveFailedResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status500InternalServerError)
{
}

/// <summary>
/// Represents a 409 Conflict response when the maximum number of games has been reached.
/// </summary>
/// <param name="message">A message describing the limit reached.</param>
public class ApiMaxGameLimitReachedResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status409Conflict)
{ }

/// <summary>
/// Represents a 400 Bad Request response for an invalid PATCH document in a game update.
/// </summary>
/// <param name="message">A message describing the issue with the PATCH document.</param>
public class ApiBadGamePatchDocumentResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status400BadRequest)
{ }

/// <summary>
/// Represents a 304 Not Modified response indicating that the resource has not changed.
/// </summary>
/// <param name="message">An optional message explaining the result.</param>
public class ApiNotModifiedResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status304NotModified)
{ }

/// <summary>
/// Represents a 422 non-processable Entity response indicating that the server could not process the request.
/// </summary>
/// <param name="message">A message explaining why the request could not be processed.</param>
public class ApiUnProcessableContentResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status422UnprocessableEntity)
{ }

/// <summary>
/// Represents a 422 non-processable Entity response when a valid request results in no actual changes.
/// </summary>
/// <param name="message">A message indicating that no changes were made.</param>
public class ApiNoChangesMadeResponse(string message)
    : ApiBaseResponse(false, message, StatusCodes.Status422UnprocessableEntity)
{ }
