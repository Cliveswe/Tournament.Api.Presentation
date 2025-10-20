using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Service.Contracts
{
    public interface IWebDependencyHealthCheck
    {
        Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);
    }
}