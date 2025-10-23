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
            
            return HealthCheckResult.Unhealthy();
        } 
    }

    protected abstract Task<HealthCheckResult> GetHealthCheckResultAsync(Stopwatch stopwatch, CancellationToken cancellationToken);
}
