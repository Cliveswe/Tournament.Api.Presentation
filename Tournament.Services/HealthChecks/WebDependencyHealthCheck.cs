// Ignore Spelling:

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Service.Contracts;

namespace Tournaments.Services.HealthChecks;

public class WebDependencyHealthCheck : IHealthCheck, IWebDependencyHealthCheck
{
    private readonly HttpClient httpClient;
    private readonly string urlToCheck;

    public WebDependencyHealthCheck(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        // Get the URL from configuration appsettings.json of fallback to default.
        urlToCheck = configuration["HealthChecks:WebDependencyUrl"] ?? "https://www.umea.se/";

        if(string.IsNullOrWhiteSpace(urlToCheck))
        {
            throw new ArgumentNullException("URL for WebDependencyHealthCheck cannot be null or empty.", nameof(urlToCheck));
        }
    }

    // Created a custom health check to check the web dependency.
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        DateTime startTimeStamp = DateTime.UtcNow;

        //if (string.IsNullOrWhiteSpace(urlToCheck))
        //{
        //    return HealthCheckResult.Degraded("No URL provided for web dependency check.");
        //}
        try
        {
            //TODO web check health move this to its on class. The class must take context cancellationToken and DateTime startTimeStamp. It should return a HealthCheck.Healthy or a HealthCheck.Unhealthy result.
            
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
                //If the url is valid and reachable, but it responses with an HTTP status code indicating an error (400, 500, etc). Return unhealthy status.
                data: new Dictionary<string, object>
                {
                    ["url"] = urlToCheck,
                    ["statusCode"] = (int)response.StatusCode
                }
                );
        }
        catch (HttpRequestException)
        {
            //TODO Decide on what to catch and what errors to display.
            return HealthCheckResult.Unhealthy($"Network error while checking web dependency! Please try again later.");
        }
        catch (Exception)
        {
            //TODO Decide on what to catch and what errors to display.
            // Valid but unreachable.
            // Malformed URL.
            return HealthCheckResult.Unhealthy($"Exception while checking web dependency! Please try again later.");
        }
    }
}
