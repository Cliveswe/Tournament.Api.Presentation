//Ignore spelling: api ok leddy middleware xml liveness readiness microservice
// -----------------------------------------------------------------------------
// File: DependencyInjectionExtensions.cs
// Summary: Provides extension methods to configure repositories, service layers,
//          lazy-loading, and Swagger XML comment integration for the Tournament API.
// Author: [Clive Leddy]
// Created: [2025-07-21]
// Last modified: [2025-08-02]
// Notes: Consolidates dependency injection configuration across repositories,
//        services, and Swagger for improved modularity, maintainability, and 
//        documentation support.
// -----------------------------------------------------------------------------


using Domain.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Service.Contracts;
using Tournaments.Infrastructure.Repositories;
using Tournaments.Services.HealthChecks;
using Tournaments.Services.Services;

namespace Tournaments.Api.Extensions;

#region ServiceExtensions

/// <summary>
/// Contains extension methods for configuring repository services and enabling
/// lazy loading support within the application's dependency injection container.
/// </summary>
public static class ServiceExtensions
{

    /// <summary>
    /// Extension method for <see cref="IServiceCollection"/> that registers the Unit of Work
    /// and repository services, including support for lazy loading of repositories.
    /// </summary>
    /// <param name="services">The service collection to which the repositories and Unit of Work are added.</param>
    public static void ConfigureRepositories(this IServiceCollection services)
    {

        //Register the unit of work.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //Register the repositories.
        services.AddScoped<ITournamentDetailsRepository, TournamentDetailsRepository>();
        services.AddScoped<IGameRepository, GameRepository>();

        //Register the repositories as lazy loaded services.
        services.AddLazy<ITournamentDetailsRepository>();
        services.AddLazy<IGameRepository>();
    }

    /// <summary>
    /// Extension method for <see cref="IServiceCollection"/> that registers the core service layer dependencies,
    /// including the <see cref="IServiceManager"/> and individual services such as tournament, game, and authentication services.
    /// Also enables lazy loading for these services by registering <see cref="Lazy{T}"/> wrappers,
    /// which defer instantiation until the service is actually needed.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the services will be added.</param>
    /// <remarks>
    /// This configuration centralizes the service layer setup, ensuring that dependent components receive properly scoped instances,
    /// and improves performance and testability by deferring service instantiation with lazy loading.
    /// </remarks>
    public static void ConfigureServiceLayerServices(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<ITournamentService, TournamentService>();
        services.AddScoped<IGameService, GameService>();

        // Register individual services with lazy loading.
        services.AddLazy<ITournamentService>();
        services.AddLazy<IGameService>();
        services.AddLazy<IAuthService>();


        //Health checks
        services.AddScoped<IWebDependencyHealthCheck, WebDependencyHealthCheck>();
        services.AddLazy<IWebDependencyHealthCheck>();
        services.AddScoped<IDatabaseConnectionHealthCheck, DatabaseConnectionHealthCheck>();
        services.AddLazy<IDatabaseConnectionHealthCheck>();

    }

}// End of Class ServiceExtensions.

#endregion

#region ServiceCollectionExtensions

/// <summary>
/// Provides extension method to add lazy loading support for services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a service to be resolved lazily using <see cref="Lazy{T}"/>, 
    /// allowing deferred instantiation until the service is actually needed.
    /// </summary>
    /// <typeparam name="TService">The type of the service to be registered.</typeparam>
    /// <param name="services">The service collection to add the lazy registration to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the lazy service registration.</returns>
    public static IServiceCollection AddLazy<TService>(this IServiceCollection services) where TService : class
    {
        return services.AddScoped(provider => new Lazy<TService>(() => provider.GetRequiredService<TService>()));
    }
}// End of Class ServiceCollectionExtensions.

#endregion ServiceCollectionExtensions

#region SwaggerServiceExtension

/// <summary>
/// Provides extension methods for configuring Swagger services, including automatic
/// inclusion of XML documentation files from known projects into the Swagger UI.
/// </summary>
/// <remarks>
/// This class centralizes Swagger configuration by including XML comments for multiple 
/// assemblies, such as the API, shared DTOs, and presentation layer. It ensures that XML 
/// documentation is only included if the corresponding files exist, avoiding runtime exceptions.
/// </remarks>
public static class SwaggerServiceExtensions
{
    /// <summary>
    /// Registers and configures Swagger services, including automatic inclusion of XML documentation 
    /// files from a predefined list of project assemblies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which Swagger services will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with Swagger services configured.</returns>
    /// <remarks>
    /// This method checks for the existence of XML files in the output directory before including them.
    /// This prevents runtime failures in environments where certain projects or XML files may not be present.
    /// </remarks>
    public static IServiceCollection AddSwaggerXmlComments(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            var basePath = AppContext.BaseDirectory;

            // Auto discover all xml files. This is a dynamic discovery of the xml documentation in each
            // project of the solution. Important: Make sure that each project has properties/Build/Output/
            // Documentation file checked. In addition make sure the Xml file documentation path is the
            // project is populated with the project path for example "Tournaments.Shared.xml". Also note
            // that the .xml is required.
            var xmlDocs = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);

            foreach (var xmlDoc in xmlDocs)
            {
                options.IncludeXmlComments(xmlDoc);
            }


        });

        return services;
    }
}

#endregion


#region HealthChecksExtension

/// <summary>
/// Provides extension methods for configuring health checks in the application.
/// </summary>
/// <remarks>
/// This static class simplifies the registration of health check services in the application's
/// dependency injection container. It supports both liveness and readiness checks, including
/// SQL Server connectivity verification.
/// </remarks>
public static class HealthChecksExtensions
{
    /// <summary>
    /// Registers health check services in the <see cref="IServiceCollection"/>, including
    /// a liveness check and a SQL Server readiness check.
    /// </summary>
    /// <param name="services">The service collection to add health checks to.</param>
    /// <remarks>
    /// - Liveness check ("self") always reports healthy.
    /// - Readiness check verifies SQL Server connectivity using a simple query.
    /// This method ensures that health checks are tagged appropriately for liveness
    /// ("liveness") and readiness ("readiness") endpoints.
    /// </remarks>
    public static void HealthChecksServiceExtensions(
        this IServiceCollection services)
    {
        // Add HttpClient factory to the services collection.
        // This is required for health checks that depend on HttpClient.
        services.AddHttpClient();

        // Ensure that the required health check services are registered with dependency injection container.
        // Use the AddHealthChecks extension method to register endpoints to the IServiceCollection inside the configureServices.
        // Registers required health checks services. AddHealthChecks method configures a basic HTTP check that returns a 
        // 200 Ok status code with "Healthy" response when requested.
        IHealthChecksBuilder healthChecksBuilder = services.AddHealthChecks()

            // N.B. To begin with, define what constitutes a healthy status for each microservice.
            // To take full advantage of health checks, use tags to group or filter health checks.
            // To use tags, you will need to specify register them in app.MapHeathChecks middleware.

            // Register what you want to check. Each AddCheck extension method configures a custom health check.
            //
            // Healthy:   Indicates the application is operating correctly.
            // Degraded:  Indicates the service is live, but some functionality may be unavailable.
            // Unhealthy: Indicates that the application may be unable to operate or is not ready. By default
            // if for example a database is down or unreachable then the service is likely unable to function.
            // We can customize the health status to opt to use the degraded status instead. This may be useful
            // if the service can still operate without the database.
            //
            // liveness check 
            //
            // As long as the application can handle HTTP basic requests/responses then the application
            // is fundamentally live!
            // if this fails usually indicates a service issue such as a hung or crashed application.
            // A failing instance suggests that restarting the application usually fixes the problem.
            //
            // Readiness check.
            //
            // Tests more than just the ability to respond to HTTP liveness requests. May take longer to return
            // as healthy. Thus readiness probes are performed periodically during the life of the service.
            // Readiness health check failures signal that the service cannot handle requests, but does not force
            // a restart of the service.
            //
            // name:          Identifies the health check.
            // timeout:       Maximum duration the health check is allow to run.
            // failureStatus: Returned status when the health check fails.
            // tags:          An array of tags for the health check, can be helpful for filtering.
            //

            //
            // Add health check to confirm the availability of a dependency through the Entity Framework
            // context.
            //

            //
            // Liveness check - the basic health prob is a self check.
            //
            // Health check prob as a self check.
            .AddCheck(
            name: "Api Self Check.",
            check: () => HealthCheckResult.Healthy(),
            timeout: TimeSpan.FromSeconds(3),
            tags: new[] { "liveness" })

            // Check a web dependency.
            // Register an instance of health check to check dynamically web dependency, use a factory method.
            .AddCheck<WebDependencyHealthCheck>(
            name: "ArchiveService  Web Dependency Check.", //name that identifies the health check.
            timeout: TimeSpan.FromSeconds(3), // The maximum duration the health check is allow to run.
            failureStatus: HealthStatus.Degraded, // status returned when the health check fails.
            tags: new[] { "liveness" } // An array of tags for the health check, can be helpful for filtering.
            )

            //
            // Readiness check - test more than just the ability to respond to HTTP liveness requests.
            //
            // May take longer to return as healthy. Thus readiness probes are performed periodically during the life
            // of the service. Readiness health check failures signal that the service cannot handle requests, but
            // does not force a restart of the service.
            //
            //sp => new DatabaseConnectionHealthCheck(sp.GetRequiredService<IConfiguration>()),
            .AddCheck<DatabaseConnectionHealthCheck>(
            name: "ArchiveService Database Dependency Check.",
            failureStatus: HealthStatus.Degraded,
            timeout: TimeSpan.FromSeconds(5),
            tags: new[] { "readiness" }
            );

    }
}// End of Class HealthChecksExtensions.
#endregion
