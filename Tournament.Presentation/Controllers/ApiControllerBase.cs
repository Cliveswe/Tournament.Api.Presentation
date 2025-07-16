// Ignore Spelling: Api

using Domain.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tournaments.Presentation.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ApiControllerBase : ControllerBase
{
    private IResult CreateProblemResult(string title, string detail, int statusCode)
    {
        return Results.Problem(
            title: title,
            detail: detail,
            statusCode: statusCode,
            instance: HttpContext.Request.Path
        );
    }


    [NonAction]
    public ActionResult ProcessError(ApiBaseResponse baseResponse)
    {

        return baseResponse switch
        {
            //Checks type and assigns instance to notFound
            ApiNotFoundResponse notFound => NotFound(CreateProblemResult(
                "Not found", notFound.Message, notFound.StatusCode)),

            //Checks type and assigns instance to limitReached
            MaxGameLimitReachedResponse limitReached => Conflict(CreateProblemResult(
                "Maximum game limit reached", limitReached.Message, limitReached.StatusCode)),
            GameAlreadyExistsResponse alreadyExists => Conflict(CreateProblemResult(
                "Conflict", alreadyExists.Message, alreadyExists.StatusCode)),

            GameSaveFailedResponse saveFailed => StatusCode(saveFailed.StatusCode, CreateProblemResult(
                "Save Failed", saveFailed.Message, saveFailed.StatusCode)),

            _ => throw new NotImplementedException()
        };
    }
}
