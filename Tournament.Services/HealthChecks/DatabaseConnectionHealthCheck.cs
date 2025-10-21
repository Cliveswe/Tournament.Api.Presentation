

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Service.Contracts;
using System.Data.Common;

namespace Tournaments.Services.HealthChecks;

// Sample SQL Connection Health Check
public class DatabaseConnectionHealthCheck : IHealthCheck, IDatabaseConnectionHealthCheck
{
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
            ?? "SELECT 1";
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
    {
        DateTime startTimeStamp = DateTime.UtcNow;
        try
        {

            await using SqlConnection connection = new(ConnectionString);
            await connection.OpenAsync(cancellationToken);


            if (!string.IsNullOrWhiteSpace(TestQuery))
            {
                await using SqlCommand command = connection.CreateCommand();
                command.CommandText = TestQuery;

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            TimeSpan duration = DateTime.UtcNow - startTimeStamp;

            return HealthCheckResult.Healthy(
                description: $"Database connection is healthy.",
                data: new Dictionary<string, object>
                {
                    //Safety first: extract the database name from the connection string.
                    ["database"] = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog,
                    ["statusCode"] = 0,// 0 indicates success
                    ["responseTimeMs"] = duration.TotalMilliseconds
                });
        }
        catch (DbException ex)
        {
            TimeSpan duration = DateTime.UtcNow - startTimeStamp;
            return HealthCheckResult.Unhealthy(
                description: $"Database connection failed.",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    //Safety first: extract the database name from the connection string.
                    ["database"] = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog,
                    ["responseTimeMs"] = duration.TotalMilliseconds,
                    // check if ex is of type SqlException, if so cast it to SqlException and assign it to sqlEx.
                    // if it is SqlException it uses the Number property from SqlException. Number is the
                    // SQL Server error code. Otherwise, it assigns -1 to indicate an unknown error code. 
                    ["errorCode"] = ex is SqlException sqlEx ? sqlEx.Number : -1
                });
        }
    }
}
