// -----------------------------------------------------------------------------
// File: ApplicationBuilderExtensions.cs
// Summary: Provides extension methods for IApplicationBuilder to support
//          database seeding during application startup.
// Author: [Clive Leddy]
// Created: [2025-07-13]
// Notes: Wraps seeding logic in an extension method for cleaner startup code.
// -----------------------------------------------------------------------------

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Tournaments.Infrastructure.Data;
using HealthExt = Tournaments.Shared.HealthChecks.HealthCheckJsonWriter;
using Microsoft.AspNetCore.Http;


namespace Tournaments.Api.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="IApplicationBuilder"/> interface,
/// including methods to seed initial data into the database.
/// </summary>
public static class ApplicationBuilderExtensions
{
    #region SeedDataAsync
    /// <summary>
    /// Runs asynchronous database seed operations during application startup.
    /// </summary>
    /// <param name="builder">The application builder instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous seed operation.</returns>
    public static async Task SeedDataAsync(this IApplicationBuilder builder)
    {
        await SeedData.SeedDataAsync(builder);
    }

    #endregion

    #region HealthChecksMiddleware

    /// <summary>
    /// Provides extension methods for adding health check middleware to the application's request pipeline.
    /// In other words, registers the health check endpoint.
    /// </summary>
    /// <remarks>This middleware enables the application to expose a health check endpoint, which can be used to
    /// monitor the application's health status. By default, the health check endpoint is configured at the path
    /// "/health", "/health/live" or "/health/ready".
    /// Note: Health checks has no swagger integration by default. You need to add it manually if required.
    /// </remarks>
    public static void HealthChecksMiddlewareExtensions(this WebApplication app)
    {
        
        // Add Health Check endpoints at the *end* of routing
        // Define routes "/health/"
        app.MapHealthChecks("/health");

        // Define liveness and readiness endpoints "/health/live" 
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("liveness")
        });

        // Define readiness endpoint "/health/ready"
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("readiness"),
            ResponseWriter = HealthExt.WriteJsonResponseAsync
        });
    }
    #endregion
    
}
