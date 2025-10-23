//Ignore Spelling:

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Tournaments.Services.HealthChecks;

public abstract class BaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        try {
            return await GetHealthCheckResultAsync(stopwatch, cancellationToken);

        } catch (Exception ex) {
            
            //TODO create a relevant unhealthy message.
            return HealthCheckResult.Unhealthy($"Health check failed {ex.Message}");
        } 
    }

    protected abstract Task<HealthCheckResult> GetHealthCheckResultAsync(Stopwatch stopwatch, CancellationToken cancellationToken);
}
