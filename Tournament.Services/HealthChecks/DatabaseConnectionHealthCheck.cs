

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Service.Contracts;
using System.Data.Common;

namespace Tournaments.Services.HealthChecks;

// Sample SQL Connection Health Check
public class DatabaseConnectionHealthCheck : IHealthCheck, ISqlConnectionHealthCheck
{
    private const string DefaultTestQuery = "Select 1";

    public string ConnectionString { get; }

    public string TestQuery { get; }

    public DatabaseConnectionHealthCheck(string connectionString, string testQuery = DefaultTestQuery)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        TestQuery = testQuery;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
    {
        DateTime startTimeStamp = DateTime.UtcNow;
        try
        {

            await using SqlConnection connection = new SqlConnection(ConnectionString);
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
