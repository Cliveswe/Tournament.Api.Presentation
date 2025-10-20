using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Tournaments.Shared.HealthChecks
{
    public interface IWebDependencyHealthCheck
    {
        Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);
    }
}