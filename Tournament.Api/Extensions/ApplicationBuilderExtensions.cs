//Ignore Spelling: leddy middleware liveness hc
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
using HealthExt = Tournaments.Services.HealthChecks.HealthCheckJsonWriter;
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

        // Add Health Check endpoints at the *end* of routing.
        // Register the endpoint route using the MapHealthChecks extension method.
        // This requires to define the route pattern(s) that we want to use for our health check endpoint(s) i.e. "/health/..."
        // Important: The route pattern "must" be prefixed with a "/". 
        //
        // We can provide some health check options to control the health check as a second argument to the MapHealthChecks
        // method called HealthCheckOptions.
        // 
        // HealthStatus is an enum with "HealthStatus.Unhealthy" considered as the default status. Unhealthy = 0, Degraded = 1 and 
        // Healthy = 2. However, HealthStatus has been overridden with StatusCodes that, in this scenario, are relevant for this 
        // particular use case.
        //

        //
        // Define liveness and readiness endpoints.
        //
        // Endpoint "/health/ready"
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            // A method that writes a response onto the HTTP context using data from the health check report.
            ResponseWriter = HealthExt.WriteJsonResponse,

            ResultStatusCodes =
            {
                [HealthStatus.Unhealthy] = StatusCodes.Status500InternalServerError,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Healthy] = StatusCodes.Status200OK
            }

        });

        // Endpoint "/health/ping" 
        app.MapHealthChecks("/health/ping");

        // Endpoint "/health/live" 
        app.MapHealthChecks("/health/live", BuildLivenessHealthCheckOptions());

        // Endpoint "/health/ready"
        app.MapHealthChecks("/health/ready", BuildLivenessHealthCheckOptions("readiness"));
    }

    //
    // Health check options to control the health check, provides opportunity to customize health check behavior.
    // The Predicate property can be used to filter the set of health checks which will be executed. In this
    // case the array of tags used for filtering. This now allows relatively fine-grain control over which health
    // check should be allowed to run for each mapped health check endpoint.
    // "services.AddHealthChecks().AddCheck<SomeDIClass>(
    // name: ...
    // timeout: ...
    // failureStatus: ...
    // tags:...)"
    //
    private static HealthCheckOptions BuildLivenessHealthCheckOptions(string tagString = "liveness")
    {
        return new HealthCheckOptions
        {
            // Control what health check (hc) to run. In other words, to run a subset of health checks,
            // provide a function that returns a boolean to the Predicate option. The predicate is use
            // to filter the set of heath checks executed. Note: if the Predicate is null the "HealthCheckMiddleware" 
            // will run all registered health checks, this is also the default behavior.
            Predicate = hc => hc.Tags.Contains(tagString), 

            // A method that writes a response onto the HTTP context using data from the health check report.
            ResponseWriter = HealthExt.WriteJsonResponse, 
            
            ResultStatusCodes =
            {
                [HealthStatus.Unhealthy] = StatusCodes.Status500InternalServerError,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Healthy] = StatusCodes.Status200OK
            }
        };
    }
   
    #endregion

}
