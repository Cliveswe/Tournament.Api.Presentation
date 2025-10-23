

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Service.Contracts;
using System.Data.Common;
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
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            // TODO DB check health move this to its on class. The class must take context cancellationToken and DateTime startTimeStamp. It should return a HealthCheck.Healthy or a HealthCheck.Unhealthy result.


            //var res =  await GetHealthCheckResultAsync(startTimeStamp, connection, cancellationToken);
            var res = await GetHealthCheckResultAsync(stopwatch, cancellationToken);
           
            return res;
        }
        catch (DbException ex)
        {
            //TODO Decide on what to catch and what errors to display.
            //TimeSpan duration = DateTime.UtcNow - startTimeStamp;
            stopwatch.Stop();
            return HealthCheckResult.Unhealthy(
                description: $"Database connection failed.",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    //Safety first: extract the database name from the connection string.
                    ["database"] = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog,
                    ["responseTimeMs"] = stopwatch.ElapsedMilliseconds,
                    // check if ex is of type SqlException, if so cast it to SqlException and assign it to sqlEx.
                    // if it is SqlException it uses the Number property from SqlException. Number is the
                    // SQL Server error code. Otherwise, it assigns -1 to indicate an unknown error code. 
                    ["errorCode"] = ex is SqlException sqlEx ? sqlEx.Number : -1
                });
        }
    }

    private async Task<HealthCheckResult> GetHealthCheckResultAsync(Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        int response;
        stopwatch.Start();

        using (SqlConnection connection = new(ConnectionString))
        {
            await connection.OpenAsync(cancellationToken);

            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = TestQuery;// Default query 

                //Execute the command
                response = await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        stopwatch.Stop();

        return HealthCheckResult.Healthy(
            description: $"Database connection is healthy.",
            data: new Dictionary<string, object>
            {
                //Safety first: extract the database name from the connection string.
                ["database"] = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog,
                ["statusCode"] = response,// 0 indicates success, -1 the command executed was a DDL rather than a DML!!!
                ["responseTimeMs"] = stopwatch.ElapsedMilliseconds
            });
    }

    protected override Task<HealthCheckResult> GetHealthCheckResultAsync(Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
