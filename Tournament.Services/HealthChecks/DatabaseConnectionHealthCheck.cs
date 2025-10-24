using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Service.Contracts;
using System.Diagnostics;

namespace Tournaments.Services.HealthChecks;

// Sample SQL Connection Health Check
public class DatabaseConnectionHealthCheck : BaseHealthCheck, IDatabaseConnectionHealthCheck
{
    //TODO Re-factor this class to a abstract base class.
    private const string DefaultTestQuery = "Select 1";

    public string ConnectionString { get; }

    public string TestQuery { get; }

    public DatabaseConnectionHealthCheck(IConfiguration configurationManager)
    {
        //IConfiguration is injected automatically by asp.net core dependency injection (DI)
        // It gives one access to all configuration values for example: appsettings.json environment variables etc.
        // Important -> This parameter lets health check read from configuration directly.

        // Get the key name of the connection string from appsettings.json. This ensures that the configuration is valid early!
        string connKey = configurationManager["HealthChecks:ConnectionStringKey"]
            ?? throw new ArgumentException($"HealthChecks:ConnectionStringKey not found in configuration.");

        // Retrieve the actual database connection string. This looks up the named connection string from the ConnectionStrings in appsettings.json.
        string connectionString = configurationManager.GetConnectionString(connKey)
            ?? throw new ArgumentException($"Connection string '{connKey}' not found.");

        // Assign the final connection string.
        ConnectionString = connectionString;

        // Read a (default) SQL test query.
        TestQuery = configurationManager["HealthChecks:TestQuery"]
            ?? DefaultTestQuery;
    }

    protected override async Task<HealthCheckResult> GetHealthCheckResultAsync(Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        int response;

        stopwatch.Start();

        using (SqlConnection connection = new(ConnectionString))
        {
            await connection.OpenAsync(cancellationToken);

            // Execute the command.
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = TestQuery;// Default query 
                response = await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        stopwatch.Stop();
        return HealthyReport(stopwatch, response.ToString());
    }

    protected override HealthCheckResult HealthyReport(Stopwatch stopwatch, string response) => HealthCheckResult.Healthy(
            description: $"XDatabase connection is healthy.",
            data: new Dictionary<string, object>
            {
                //Safety first: extract the database name from the connection string.
                ["database"] = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog,
                ["statusCode"] = response,// 0 indicates success, -1 the command executed was a DDL rather than a DML!!!
                ["responseTimeMs"] = stopwatch.ElapsedMilliseconds
            });

    protected override HealthCheckResult UnHealthyReport(Stopwatch stopwatch, int response)
    {
        throw new NotImplementedException();
    }

    protected override HealthCheckResult DegradedReport(Stopwatch stopwatch, int response)
    {
        throw new NotImplementedException();
    }
}
