using Microsoft.EntityFrameworkCore;
using Tournaments.Api.Extensions;
using Tournaments.Infrastructure.Data;


// Ignore Spelling: Api

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
            builder.Services.AddSwaggerGen();

            //Not needed because using Unit of Work creates instances of the repositories.
            //builder.Services.AddScoped<ITournamentDetailsRepository, TournamentDetailsRepository>();
            //builder.Services.AddScoped<IGameRepository, GameRepository>();
            //builder.Services.AddScoped<IUoW, UoW>();

            builder.Services.ConfigureServiceLayerServices();
            // Register repositories and Unit of Work with lazy loading
            builder.Services.ConfigureRepositories();

            builder.Services.AddAutoMapper(typeof(TournamentMappings));
            var app = builder.Build();

            app.ConfigureExceptionHandler();

            await app.SeedDataAsync();

            // Configure the HTTP request pipeline.
            if(app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
