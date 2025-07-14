// Ignore Spelling: Api

using Domain.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tournaments.Presentation.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ApiControllerBase : ControllerBase
{
    [NonAction]
    public ActionResult ProcessError(ApiBaseResponse baseResponse)
    {
        return baseResponse switch
        {
            //Microsoft.AspNetCore.Mvc.NewtonsoftJson is a requirement!
            ApiNotFoundResponse => NotFound(Results.Problem
            (
                detail: ((ApiNotFoundResponse)baseResponse).Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Not found.",
                instance: HttpContext.Request.Path
                )),
            _ => throw new NotImplementedException()
        };
    }
}
