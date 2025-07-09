// -----------------------------------------------------------------------------
// File: ServiceExtensions.cs
// Summary: Provides extension methods to configure repository services and support
//          lazy loading for dependency injection within the Tournament API.
// Author: [Clive Leddy]
// Created: [2025-07-04]
// Notes: Simplifies the registration of repositories and Unit of Work pattern
//        implementations by encapsulating service registrations and enabling
//        lazy-loaded repositories for improved performance and resource management.
// -----------------------------------------------------------------------------


using Domain.Contracts;
using Tournament.Data.Repositories;

namespace Tournament.Api.Extensions;

#region ServiceExtensions
/// <summary>
/// Contains extension methods for configuring repository services and enabling
/// lazy loading support within the application's dependency injection container.
/// </summary>
public static class ServiceExtensions
{

    /// /// <summary>
    /// Extension method for <see cref="IServiceCollection"/> that registers the Unit of Work
    /// and repository services, including support for lazy loading of repositories.
    /// </summary>
    /// <param name="services">The service collection to which the repositories and Unit of Work are added.</param>
    public static void ConfigureRepositories(this IServiceCollection services)
    {

        //Register the unit of work.
        services.AddScoped<IUoW, UoW>();

        //Register the repositories.
        services.AddScoped<ITournamentDetailsRepository, TournamentDetailsRepository>();
        services.AddScoped<IGameRepository, GameRepository>();

        //Register the repositories as lazy loaded services.
        services.AddLazy<ITournamentDetailsRepository>();
        services.AddLazy<IGameRepository>();
    }

    public static void ConfigureServiceLayerServices(this IServiceCollection services)
    {
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