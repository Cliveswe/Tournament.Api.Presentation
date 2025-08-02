// -----------------------------------------------------------------------------
// File: ApplicationBuilderExtensions.cs
// Summary: Provides extension methods for IApplicationBuilder to support
//          database seeding during application startup.
// Author: [Clive Leddy]
// Created: [2025-07-13]
// Notes: Wraps seeding logic in an extension method for cleaner startup code.
// -----------------------------------------------------------------------------

using Tournaments.Infrastructure.Data;

namespace Tournaments.Api.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="IApplicationBuilder"/> interface,
/// including methods to seed initial data into the database.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Runs asynchronous database seed operations during application startup.
    /// </summary>
    /// <param name="builder">The application builder instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous seed operation.</returns>
    public static async Task SeedDataAsync(this IApplicationBuilder builder)
    {
        await SeedData.SeedDataAsync(builder);
    }


}
