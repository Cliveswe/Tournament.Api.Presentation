using Microsoft.EntityFrameworkCore;
using Tournaments.Api.Extensions;
using Tournaments.Infrastructure.Data;


// Ignore Spelling: Api xml liveness

namespace Tournaments.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<TournamentApiContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("TournamentApiContext") ?? throw new InvalidOperationException("Connection string 'TournamentApiContext' not found.")));

            // Add services to the container.
            builder.Services.AddControllers(opt => opt.ReturnHttpNotAcceptable = true)
                .AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //Implementing xml comment on action in a controller that is then displayed in Swagger.
            builder.Services.AddSwaggerXmlComments();
            /*builder.Services.AddSwaggerGen(
                options =>
                {
                    var basePath = AppContext.BaseDirectory;

                    // Helper method to conditionally add XML comments
                    void TryIncludeXml(string fileName)
                    {
                        var fullPath = Path.Combine(basePath, fileName);
                        if(File.Exists(fullPath)) {
                            options.IncludeXmlComments(fullPath);
                        }
                    }

                    // Main API project
                    TryIncludeXml("Tournaments.Api.xml");

                    // Shared DTOs or Domain Models
                    TryIncludeXml("Tournaments.Shared.xml");

                    // Controller/Presentation Layer
                    TryIncludeXml("Tournaments.Presentation.xml");

                    // Shared pagination MetaData
                    //TryIncludeXml("Tournaments.Shared.xml");

                    // Optionally include other layers like:
                    // TryIncludeXml("Tournaments.Infrastructure.xml");
                    // TryIncludeXml("Tournaments.Application.xml");

                    // You can also configure other Swagger options here if needed

                });*/

            //Not needed because using Unit of Work creates instances of the repositories.
            //builder.Services.AddScoped<ITournamentDetailsRepository, TournamentDetailsRepository>();
            //builder.Services.AddScoped<IGameRepository, GameRepository>();
            //builder.Services.AddScoped<IUoW, UoW>();

            builder.Services.ConfigureServiceLayerServices();
            // Register repositories and Unit of Work with lazy loading
            builder.Services.ConfigureRepositories();

            builder.Services.AddAutoMapper(typeof(TournamentMappings));

            //
            //Health Checks
            //
            // Ensure that the required health check services are registered with dependency injection container.
            // Here is a custom Service extension that uses the AddHealthChecks extension method.
            //
            builder.Services.HealthChecksServiceExtensions();

            var app = builder.Build();

            app.ConfigureExceptionHandler();

            await app.SeedDataAsync();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            //
            // Health Checks
            //
            // Map endpoints to expose our liveness and readiness health checks.
            // Here we add a custom WebApplication extension that registers routes to respond to our liveness 
            // and readiness health checks.
            //
            app.HealthChecksMiddlewareExtensions();

            app.Run();
        }
    }
}
