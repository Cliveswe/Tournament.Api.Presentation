// Ignore Spelling: Api Timestamp

// -----------------------------------------------------------------------------
// File: ApiControllerBase.cs
// Summary: Serves as a shared base class for API controllers, offering a consistent
//          way to translate domain-level <see cref="ApiBaseResponse"/> objects into
//          structured <see cref="ProblemDetails"/> HTTP responses.
// Author: [Clive Leddy]
// Created: [2025-07-14]
// Last Modified: [2025-08-02]
// Notes: Intended for reuse by resource-specific controllers. Encapsulates error
//        translation logic for uniform API behavior and enhanced maintainability.
// -----------------------------------------------------------------------------

using Domain.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tournaments.Presentation.Controllers;

/// <summary>
/// Serves as a base API controller providing shared error-handling logic for derived controllers.
/// Converts <see cref="ApiBaseResponse"/> instances into standardized <see cref="ProblemDetails"/> results
/// to ensure consistent error response formatting across the API.
/// </summary>
/// <remarks>
/// Intended to be inherited by resource-specific API controllers (e.g., GameController, TournamentController).
/// Helps centralize response shaping and improves maintainability by encapsulating reusable behavior.
/// </remarks>
//[Route("api/[controller]")]
[ApiController]
public class ApiControllerBase : ControllerBase
{

    /// <summary>
    /// Constructs a <see cref="ProblemDetails"/> object with the given error metadata and a timestamp.
    /// </summary>
    /// <param name="title">A short, human-readable summary of the error.</param>
    /// <param name="detail">A more detailed explanation of the error.</param>
    /// <param name="statusCode">The HTTP status code associated with the error.</param>
    /// <param name="timestamp">The timestamp indicating when the error occurred.</param>
    /// <returns>A populated <see cref="ProblemDetails"/> instance.</returns>
    private ProblemDetails CreateProblemResult(string title, string detail, int statusCode, DateTime timestamp)
    {
        // Built-in properties of the Microsoft.AspNetCore.Mvc.ProblemDetails class.
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = HttpContext.Request.Path,
        };

        problemDetails.Extensions["Timestamp"] = timestamp;

        return problemDetails;
    }


    /// <summary>
    /// Converts an <see cref="ApiBaseResponse"/> instance into a standardized <see cref="ProblemDetails"/> response,
    /// mapping specific subclasses to appropriate HTTP status codes and messages.
    /// </summary>
    /// <param name="baseResponse">The response object containing error details.</param>
    /// <returns>An <see cref="ObjectResult"/> containing a structured <see cref="ProblemDetails"/> payload.</returns>
    [NonAction]
    public ActionResult ProcessError(ApiBaseResponse baseResponse)
    {
        /*
          NB: Kräver Microsoft.AspNetCore.Mvc.NewtonsoftJson
         */
        ProblemDetails problem = baseResponse switch
        {
            ApiNotFoundResponse notFound => CreateProblemResult(
                "Not found", notFound.Message!, notFound.StatusCode, notFound.Timestamp),

            ApiMaxGameLimitReachedResponse limitReached => CreateProblemResult(
                "Maximum game limit reached", limitReached.Message!, limitReached.StatusCode, limitReached.Timestamp),

            ApiAlreadyExistsResponse alreadyExists => CreateProblemResult(
                "Conflict", alreadyExists.Message!, alreadyExists.StatusCode, alreadyExists.Timestamp),

            ApiSaveFailedResponse saveFailed => CreateProblemResult(
                "Save Failed", saveFailed.Message!, saveFailed.StatusCode, saveFailed.Timestamp),

            ApiNoChangesMadeResponse noChangesMade => CreateProblemResult(
                "No Changes Made", noChangesMade.Message!, noChangesMade.StatusCode, noChangesMade.Timestamp),

            ApiUnProcessableContentResponse unprocessable => CreateProblemResult(
                "Non-processable Content", unprocessable.Message!, unprocessable.StatusCode, unprocessable.Timestamp),

            // A generic response as an alternative of a "Throw"
            _ => CreateProblemResult(
                "Error", baseResponse.Message ?? "An error occurred.", baseResponse.StatusCode, baseResponse.Timestamp)
        };

        return new ObjectResult(problem)
        {
            StatusCode = problem.Status
        };
    }
    //Use this when you expect a typed result back from the service (like a DTO or collection of DTOs).
    protected ActionResult<T> HandleResponse<T>(ApiBaseResponse response) =>
        response.Success
        ? Ok(response.GetOkResult<T>()) // Return the typed success result (e.g., a DTO or collection)
        : ProcessError(response);// If delete was not successful

    //Use this when you're not expecting a typed DTO, but just want to return a general Ok(...) or
    //handle errors. This is common in PUT, DELETE, etc., where success might just mean an operation completed.
    protected ActionResult HandleResponse(ApiBaseResponse response) =>
        response.Success
        ? Ok(response) // Return the deleted object wrapped in ApiOkResponse
        : ProcessError(response);// If delete was not successful
}
