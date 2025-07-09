using Bogus;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tournament.Data.Data;
public static class SeedData
{
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
                var tournaments = GenerateTournaments(4);
                dbContext.AddRange(tournaments);
                await dbContext.SaveChangesAsync();
            } catch(Exception) {
                // Log the exception (ex) here if needed
                throw;
            }
        }
    }

    private static List<TournamentDetails> GenerateTournaments(int numberOfTournaments)
    {
        var faker = new Faker<TournamentDetails>("sv")
            .Rules((f, c)=>
            {
                //c.Id = f.IndexFaker + 1; // Ensures unique Ids starting from 1
                c.Title = f.Lorem.Sentence(3);
                c.StartDate = f.Date.Future(1, DateTime.Now);
                c.Games = GenerateGames(f.Random.Int(min: 1, max: 5), c.StartDate);
                //c.Games = new Faker<Games>()
                //    .RuleFor(g => g.Id, f => f.IndexFaker + 1)
                //    .RuleFor(g => g.Title, f => f.Lorem.Sentence(2))
                //    .RuleFor(g => g.Time, f => f.Date.Future(1, c.StartDate))
                //    .Generate(5); // Generates 5 games per tournament
            });

        return faker.Generate(numberOfTournaments);
    }

    private static ICollection<Game> GenerateGames(int numberOfGames, DateTime startDate)
    {
        var gamesFaker = new Faker<Game>("sv")
        .Rules((f, g) =>
        {
            g.Title = string.Join(" ", f.Lorem.Words(2));
            g.Time = f.Date.Future(1, startDate);
        });

        return gamesFaker.Generate(numberOfGames);
    }
}
