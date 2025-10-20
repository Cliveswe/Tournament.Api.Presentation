using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Service.Contracts
{
    public interface ISqlConnectionHealthCheck
    {
        string ConnectionString { get; }
        string TestQuery { get; }

        Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);
    }
}