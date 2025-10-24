//Ignore Spelling:

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Tournaments.Services.HealthChecks;

// Health checks are created by implementing the IHealthCheck interface. The CheckHealthAsync method returns a HealthCheckResult
// that indicates the health as UnHealthy, Degraded or Healthy. 
// CheckHealthAsync is the starting point where the health check's logic is placed. GetHealthCheckResultAsync is where custom logic
// to test a particular services health.
public abstract class BaseHealthCheck : IHealthCheck
{
    /// <summary>
    /// Runs the health check, returning the status of the component being checked. This is the health check's logic.
    /// </summary>
    /// <param name="context"> A context object associated with the current execution.</param>
    /// <param name="cancellationToken">A token that can be use to cancel the health check.</param>
    /// <returns>A Task<HealthCheckResult> that completes when the health check has finished, with the status of the component being checked.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            return await GetHealthCheckResultAsync(stopwatch, cancellationToken);

        }
        catch (Exception ex)
        {

            //TODO create a relevant unhealthy message.
            return HealthCheckResult.Unhealthy($"Health check failed {ex.Message}");
        }
    }

    //TODO Time to make a decision on what to display. EX create a virtual methods to display default health check and allow derived classes to override this base class!!! Or continue with abstract class so that derived class must use inheritance to access the base class methods.

    // This is the custom health check logic for a specific use case.
    protected abstract Task<HealthCheckResult> GetHealthCheckResultAsync(Stopwatch stopwatch, CancellationToken cancellationToken);

    //TODO Decide the best path to display the health check results.
    protected virtual HealthCheckResult UnHealthyReport(Stopwatch stopwatch, string response = "0") =>
        HealthCheckResult.Unhealthy(
            description: "An UnHealthy result!",
            data: new Dictionary<string, object>()
            );

    protected virtual HealthCheckResult DegradedReport(Stopwatch stopwatch, string response = "0") =>
        HealthCheckResult.Degraded(
            description: "An Degraded result!",
            data: new Dictionary<string, object>()
            );

    protected virtual HealthCheckResult HealthyReport(Stopwatch stopwatch, string response = "0") =>
        HealthCheckResult.Healthy(
            description: "A Healthy result!",
            data: new Dictionary<string, object>()
            );
}
