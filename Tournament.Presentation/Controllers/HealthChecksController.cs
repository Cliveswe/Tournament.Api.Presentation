// -------------------------------------------------------------------------------------
// File: HealthChecksController.cs
// Summary: Provides HTTP API endpoints to retrieve the health status of the application,
//          including readiness and liveness checks. Returns detailed JSON results
//          for monitoring and integration with dashboards or automated probes.
// <author> [Clive Leddy] </author>
// <created> [2025-10-17] </created>
// <lastModified> [2025-10-17] </lastModified>
// Notes: Integrates with ASP.NET Core Health Checks and uses a shared JSON writer
//        to produce structured responses. Supports Swagger annotations for documentation.
//

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Annotations;
using HealthExt = Tournaments.Services.HealthChecks.HealthCheckJsonWriter;

namespace Tournaments.Presentation.Controllers;

/// <summary>
/// Provides endpoints to check the health status of the API.
/// </summary>
/// <remarks>
/// This controller exposes readiness and liveness health checks for monitoring purposes.
/// The <c>GetHealthDetails</c> endpoint returns the readiness status including details
/// of all registered health checks in JSON format.
/// </remarks>
[ApiController]
[Route("health")]
[Produces("application/json")]
public class HealthChecksController(HealthCheckService healthCheckService) : ApiControllerBase
{
    /// <summary>
    /// Retrieves the readiness health status of the application.
    /// </summary>
    /// <returns>
    /// A JSON object containing the overall status and detailed results of each health check.
    /// </returns>
    /// <remarks>
    /// This endpoint queries all health checks tagged with "readiness" and returns a structured
    /// JSON response similar to the built-in "/health/ready" endpoint. It is intended for use
    /// in monitoring dashboards or automated health probes.
    /// </remarks>
    /// <response code="200">The application is healthy and all checks passed. Returns JSON with health details.</response>
    /// <response code="503">One or more health checks failed. Returns JSON with failing health check details.</response>
    [HttpGet("details")]
    [SwaggerOperation(
            Summary = "Get readiness health status",
            Description = "Returns detailed JSON results for all health checks tagged with 'readiness'.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task GetHealthDetails()
    {
        var report = await healthCheckService.CheckHealthAsync(
            check => check.Tags.Contains("readiness"));

        await HealthExt.WriteJsonResponse(HttpContext, report);
    }
}
