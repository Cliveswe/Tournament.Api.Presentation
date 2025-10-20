using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Tournaments.Shared.HealthChecks
{
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
            if (string.IsNullOrWhiteSpace(urlToCheck)) {
                return HealthCheckResult.Degraded("No URL probvided for web dependency check.");
            }
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(urlToCheck, cancellationToken);
                if (response.IsSuccessStatusCode)
                    return HealthCheckResult.Healthy($"Web dependency {urlToCheck} is healthy!");

                // Valid but returns error status code.
                //If the url is valid and reachable, but it respondes with an HTTP status code indicating an error (400, 500, etc). Return unhealthy status.
                return HealthCheckResult.Unhealthy($"Web dependency {urlToCheck} returned an error status code: {response.StatusCode}!");

            }
            catch 
            {
                // Valid but unreachable.
                // Malformed URL.
                return HealthCheckResult.Unhealthy($"Exception while checking {urlToCheck} web dependency!");
            }
        }
    }
}
