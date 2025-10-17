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

namespace Tournaments.Shared.HealthChecks;

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
    public static async Task WriteJsonResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        await using var ms = new MemoryStream();
        await using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("status", report.Status.ToString());

            if (report.Entries.Count > 0)
            {
                writer.WriteStartArray("results");

                foreach (var (key, value) in report.Entries)
                {
                    writer.WriteStartObject();
                    writer.WriteString("key", key);
                    writer.WriteString("status", value.Status.ToString());
                    writer.WriteString("description", value.Description);

                    writer.WriteStartArray("data");
                    foreach (var (dataKey, dataValue) in value.Data.Where(d => d.Value != null))
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName(dataKey);
                        JsonSerializer.Serialize(writer, dataValue, dataValue.GetType());
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        ms.Position = 0;
        await ms.CopyToAsync(context.Response.Body);
    }
}
