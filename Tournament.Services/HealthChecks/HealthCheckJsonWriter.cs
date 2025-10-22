//Ignore Spelling: leddy middleware json charset utf liveness
// -------------------------------------------------------------------------------------
// File: HealthCheckJsonWriter.cs
// Summary: Provides a shared utility to serialize ASP.NET Core health check results
//          into structured JSON format suitable for HTTP responses. Supports both
//          readiness and liveness checks with detailed per-check data.
// <author> [Clive Leddy] </author>
// <created> [2025-10-17] </created>
// <lastModified> [2025-10-17] </lastModified>
// Notes: Designed to be reused by controllers and middleware. Produces indented JSON
//        output and handles null values gracefully. Intended for integration with
//        monitoring dashboards and automated health probes.
//

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Tournaments.Services.HealthChecks;

/// <summary>
/// Provides helper methods for serializing <see cref="HealthReport"/> objects
/// into JSON responses that can be returned from HTTP endpoints.
/// </summary>
public static class HealthCheckJsonWriter
{
    /// <summary>
    /// Writes a <see cref="HealthReport"/> to the HTTP response body as an indented JSON object.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> of the current request.</param>
    /// <param name="report">The <see cref="HealthReport"/> containing health check results.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method produces a JSON object with the overall status and an array of individual
    /// health check results, including their status, description, and optional data.
    /// It is intended for use in readiness and liveness endpoints and can be called from
    /// controllers or middleware.
    /// </remarks>
    public static Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";// Set the Json content type.

        using MemoryStream memoryStream = new MemoryStream();

        // Writing to Json using a low-level type named Utf8JsonWriter. Supports high-performance Json serialization and de-serialization.
        using (
            Utf8JsonWriter writer = new Utf8JsonWriter(
                memoryStream,
                new JsonWriterOptions { Indented = true }//pretty-print style (easy for humans to read)
                ))
        {
            writer.WriteStartObject();// Start of root object.
            writer.WriteString("status", report.Status.ToString());

            if (report.Entries.Count > 0)
            {
                // Create an array of "results"
                writer.WriteStartArray("results");// Start of results array.

                foreach ((string key, HealthReportEntry value) in report.Entries)
                {
                    writer.WriteStartObject();// Start of each health check object.
                    // A number of records per health check object.
                    writer.WriteString("service", key);
                    writer.WriteString("status", value.Status.ToString());
                    writer.WriteString("description", value.Description);

                    // Write additional data if any.
                    if (value.Data.Count > 0)
                    {
                        foreach ((string dataKey, object dataValue) in value.Data)
                        {
                            writer.WriteString(dataKey, dataValue?.ToString() ?? string.Empty);
                        }
                    }

                    writer.WriteEndObject();// End of each health check object.
                }

                writer.WriteEndArray(); // End of results array.
            }

            writer.WriteEndObject();// End of root object.
        }

        memoryStream.Position = 0;
        memoryStream.CopyToAsync(context.Response.Body);
        return Task.CompletedTask;
    }

}
