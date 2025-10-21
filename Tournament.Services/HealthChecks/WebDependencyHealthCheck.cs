using Microsoft.Extensions.Diagnostics.HealthChecks;
using Service.Contracts;

namespace Tournaments.Services.HealthChecks;

public class WebDependencyHealthCheck : IHealthCheck, IWebDependencyHealthCheck
{
    private readonly HttpClient httpClient;
    private readonly string? urlToCheck;

    public WebDependencyHealthCheck(HttpClient httpClient, string? urlToCheck = null)
    {
        this.httpClient = httpClient;
        this.urlToCheck = urlToCheck;
    }

    // Created a custom health check to check the web dependency.
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        DateTime startTimeStamp = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(urlToCheck))
        {
            return HealthCheckResult.Degraded("No URL probvided for web dependency check.");
        }
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(urlToCheck, cancellationToken);
            TimeSpan duration = DateTime.UtcNow - startTimeStamp;

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy(
                description: $"Web dependency is healthy!",
                data: new Dictionary<string, object>
                {
                    ["url"] = urlToCheck,
                    ["statusCode"] = (int)response.StatusCode,
                    ["responseTimeMs"] = duration.TotalMilliseconds
                })
                : HealthCheckResult.Unhealthy($"Web dependency returned an error status code: {(int)response.StatusCode}!",
                // Valid but returns error status code.
                //If the url is valid and reachable, but it respondes with an HTTP status code indicating an error (400, 500, etc). Return unhealthy status.
                data: new Dictionary<string, object>
                {
                    ["url"] = urlToCheck,
                    ["statusCode"] = (int)response.StatusCode
                }
                );
        }
        catch (HttpRequestException)
        {
            return HealthCheckResult.Unhealthy($"Network error while checking web dependency! Please try again later.");
        }
        catch (Exception)
        {
            // Valid but unreachable.
            // Malformed URL.
            return HealthCheckResult.Unhealthy($"Exception while checking web dependency! Please try again later.");
        }
    }
}
