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
    
    //TODO Decide the best path to display the health check results.
    protected virtual HealthCheckResult HealthyReport(Stopwatch stopwatch, string response = "0") => HealthCheckResult.Healthy(
            description: "Database connection is healthy.",
            data: new Dictionary<string, object>
            {
                //Safety first: extract the database name from the connection string.
                ["database"] = "some database ",// TODO new SqlConnectionStringBuilder(ConnectionString).InitialCatalog,
                ["statusCode"] = response,// 0 indicates success, -1 the command executed was a DDL rather than a DML!!!
                ["responseTimeMs"] = stopwatch.ElapsedMilliseconds
            });

    protected abstract HealthCheckResult UnHealthyReport(Stopwatch stopwatch, string response = "0");
    protected abstract HealthCheckResult DegradedReport(Stopwatch stopwatch, string response = "0");
}
