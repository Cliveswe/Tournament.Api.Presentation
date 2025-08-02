//Ignore Spelling: sv Lorem Bogus

// -----------------------------------------------------------------------------
// File: SeedData.cs
// Summary: Provides initial data seeding for the Tournament API database using the Bogus
//          library to generate realistic fake tournament and game data with Swedish locale.
// Author: [Clive Leddy]
// Created: [2025-07-03]
// Last modified: [2025-08-02]
// Notes:  Seeds the database only if empty, applies EF Core migrations automatically,
//         and generates randomized tournaments with associated games.
// -----------------------------------------------------------------------------


using Bogus;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tournaments.Infrastructure.Data;

/// <summary>
/// Provides methods to seed initial tournament and game data into the database.
/// Utilizes the Bogus library with Swedish locale ("sv") to generate realistic
/// fake data for testing and development purposes. Ensures data seeding occurs
/// only when the database is empty and applies pending EF Core migrations automatically.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// The number of tournaments to generate during seeding.
    /// </summary>
    private static int numberOfTournaments = 4;

    /// <summary>
    /// The minimum number of games to generate per tournament.
    /// </summary>
    private static int minNumberOfGamesPerTournaments = 1;

    /// <summary>
    /// The maximum number of games to generate per tournament.
    /// </summary>
    private static int maxNumberOfGamesPerTournaments = 5;

    /// <summary>
    /// Seeds the database with initial tournament and game data if the database is empty.
    /// Ensures the database schema is up to date by applying migrations before seeding.
    /// </summary>
    /// <param name="builder">The application builder used to access service scopes and the database context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task SeedDataAsync(IApplicationBuilder builder)
    {
        // This method is intended to seed initial data into the database.
        // Implementation details would go here, such as checking if the database is empty
        // and adding initial records if necessary.

        using(var serviceScope = builder.ApplicationServices.CreateScope()) {
            var serviceProvider = serviceScope.ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<TournamentApiContext>();

            await dbContext.Database.MigrateAsync();
            if(await dbContext.TournamentDetails.AnyAsync()) {
                return;//Database not empty, no seeding required
            }
            try {
                var tournaments = GenerateTournaments(numberOfTournaments);
                dbContext.AddRange(tournaments);
                await dbContext.SaveChangesAsync();
            } catch(Exception) {
                // Log the exception (ex) here if needed
                throw;
            }
        }
    }

    /// <summary>
    /// Generates a list of <see cref="TournamentDetails"/> populated with randomized data,
    /// including titles, start dates, and associated games.
    /// </summary>
    /// <param name="numberOfTournaments">The number of tournament entities to generate.</param>
    /// <returns>A list of <see cref="TournamentDetails"/> instances with seeded data.</returns>
    private static List<TournamentDetails> GenerateTournaments(int numberOfTournaments)
    {
        int numberOfWordsPerSentence = 3;
        int yearsToGoForward = 1;

        var faker = new Faker<TournamentDetails>("sv")
            .Rules((f, c)=>
            {
                //c.Id = f.IndexFaker + 1; // Ensures unique Ids starting from 1
                c.Title = f
                .Lorem
                .Sentence(numberOfWordsPerSentence);

                c.StartDate = f
                .Date
                .Future(yearsToGoForward, DateTime.Now);

                c.Games = GenerateGames(f
                    .Random
                    .Int(min: minNumberOfGamesPerTournaments,
                    max: maxNumberOfGamesPerTournaments),
                    c.StartDate);

                //c.Games = new Faker<Games>()
                //    .RuleFor(g => g.Id, f => f.IndexFaker + 1)
                //    .RuleFor(g => g.Title, f => f.Lorem.Sentence(2))
                //    .RuleFor(g => g.Time, f => f.Date.Future(1, c.StartDate))
                //    .Generate(5); // Generates 5 games per tournament
            });

        return faker.Generate(numberOfTournaments);
    }

    /// <summary>
    /// Generates a collection of <see cref="Game"/> entities with randomized titles and times,
    /// scheduled relative to the given tournament start date.
    /// </summary>
    /// <param name="numberOfGames">The number of games to generate.</param>
    /// <param name="startDate">The base date from which future game times are calculated.</param>
    /// <returns>A collection of <see cref="Game"/> entities with randomized data.</returns>
    private static ICollection<Game> GenerateGames(int numberOfGames, DateTime startDate)
    {
        int numberOfRamdomWords = 2;
        int yearsToGoForward = 1;

        var gamesFaker = new Faker<Game>("sv")
        .Rules((f, g) =>
        {
            g.Title = string.Join(" ", f.Lorem.Words(numberOfRamdomWords));
            g.Time = f.Date.Future(yearsToGoForward, startDate);
        });

        return gamesFaker.Generate(numberOfGames);
    }
}
