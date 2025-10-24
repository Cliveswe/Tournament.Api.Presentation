// Ignore Spelling:

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Service.Contracts;
using System.Diagnostics;

namespace Tournaments.Services.HealthChecks;

public class WebDependencyHealthCheck : BaseHealthCheck, IWebDependencyHealthCheck
{
    private readonly HttpClient httpClient;
    private readonly string urlToCheck;

    public WebDependencyHealthCheck(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        // Get the URL from configuration appsettings.json of fallback to default.
        urlToCheck = configuration["HealthChecks:WebDependencyUrl"] ?? "https://www.umea.se/";

        if (string.IsNullOrWhiteSpace(urlToCheck))
        {
            throw new ArgumentNullException("URL for WebDependencyHealthCheck cannot be null or empty.", nameof(urlToCheck));
        }
    }

    protected override async Task<HealthCheckResult> GetHealthCheckResultAsync(Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        stopwatch.Start();

        using HttpResponseMessage httpResponse = await httpClient.GetAsync(urlToCheck, cancellationToken);

        stopwatch.Stop();

        return httpResponse.IsSuccessStatusCode
            ? HealthyReport(stopwatch, httpResponse.StatusCode.ToString())
            : UnHealthyReport(stopwatch, httpResponse.StatusCode.ToString());
    }

    protected override HealthCheckResult HealthyReport(Stopwatch stopwatch, string response) => HealthCheckResult.Healthy(
            description: $"Web dependency is healthy!",
            data: new Dictionary<string, object>
            {
                ["url"] = urlToCheck,
                ["statusCode"] = response, //(int)response.StatusCode,
                ["responseTimeMs"] = stopwatch.ElapsedMilliseconds
            });

    protected override HealthCheckResult UnHealthyReport(Stopwatch stopwatch, string response) => 
        HealthCheckResult.Unhealthy($"Web dependency returned an error status code: {response}!",
            // Valid but returns error status code.
            //If the url is valid and reachable, but it responses with an HTTP status code indicating an error (400, 500, etc). Return unhealthy status.
            data: new Dictionary<string, object>
            {
                ["url"] = urlToCheck,
                ["statusCode"] = response
            });

    protected override HealthCheckResult DegradedReport(Stopwatch stopwatch, string response = "0")
    {
        throw new NotImplementedException();
    }
}
