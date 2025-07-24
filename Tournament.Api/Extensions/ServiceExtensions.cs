// -----------------------------------------------------------------------------
// File: DependencyInjectionExtensions.cs
// Summary: Provides extension methods to configure repositories, service layers,
//          lazy-loading, and Swagger XML comment integration for the Tournament API.
// Author: [Clive Leddy]
// Created: [2025-07-21]
// Notes: Consolidates dependency injection configuration across repositories,
//        services, and Swagger for improved modularity, maintainability, and 
//        documentation support.
// -----------------------------------------------------------------------------


using Domain.Contracts;
using Service.Contracts;
using Tournaments.Infrastructure.Repositories;
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

            // List of known XML documentation files from various projects
            //var xmlFiles = new[]
            //    {
            //        "Tournaments.Api.xml",
            //        "Tournaments.Shared.xml",
            //        "Tournaments.Presentation.xml",
            //        "Tournaments.Shared.Request.xml",
            //        // Add any additional XML doc files here
            //        // "Tournaments.Application.xml",
            //        // "Tournaments.Infrastructure.xml"
            //    };

            //foreach(var file in xmlFiles) {
            //    var fullPath = Path.Combine(basePath, file);
            //    if(File.Exists(fullPath)) {
            //        options.IncludeXmlComments(fullPath);
            //    }
            //}

            // Auto discover all xml files. This is a dynamic discovery of the xml documentation in each
            // project of the solution. Important: Make sure that each project has properties/Build/Output/
            // Documentation file checked. In addition make sure the Xml file documentation path is the
            // project is populated with the project path for example "Tournaments.Shared.xml". Also note
            // that the .xml is required.
            var xmlDocs = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);

            foreach(var xmlDoc in xmlDocs) {
                options.IncludeXmlComments(xmlDoc);
            }


        });

        return services;
    }
}

#endregion