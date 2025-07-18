// Ignore Spelling: Api

using Domain.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tournaments.Presentation.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ApiControllerBase : ControllerBase
{
    private ProblemDetails CreateProblemResult(string title, string detail, int statusCode, DateTime timestamp)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["Time Stamp"] = timestamp;

        return problemDetails;
    }



    [NonAction]
    public ActionResult ProcessError(ApiBaseResponse baseResponse)
    {
        ProblemDetails problem = baseResponse switch
        {
            ApiNotFoundResponse notFound => CreateProblemResult(
            "Not found", notFound.Message!, notFound.StatusCode, notFound.TimeStamp),

            MaxGameLimitReachedResponse limitReached => CreateProblemResult(
            "Maximum game limit reached", limitReached.Message!, limitReached.StatusCode, limitReached.TimeStamp),

            GameAlreadyExistsResponse alreadyExists => CreateProblemResult(
            "Conflict", alreadyExists.Message!, alreadyExists.StatusCode, alreadyExists.TimeStamp),

            GameSaveFailedResponse saveFailed => CreateProblemResult(
            "Save Failed", saveFailed.Message!, saveFailed.StatusCode, saveFailed.TimeStamp),

            NoChangesMadeResponse noChangesMade => CreateProblemResult(
            "No Changes Made", noChangesMade.Message!, noChangesMade.StatusCode, noChangesMade.TimeStamp),

            UnProcessableContentResponse unprocessable => CreateProblemResult(
            "Non-processable Content", unprocessable.Message!, unprocessable.StatusCode, unprocessable.TimeStamp),

            // A generic response as an alternative of a "Throw"
            _ => CreateProblemResult(
            "Error", baseResponse.Message ?? "An error occurred.", baseResponse.StatusCode, baseResponse.TimeStamp)
        };

        return new ObjectResult(problem)
        {
            StatusCode = problem.Status
        };
    }

}
