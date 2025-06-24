using Tournament.Data.Data;
namespace Tournament.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task SeedDataAsync(this IApplicationBuilder builder)
    {
        await SeedData.SeedDataAsync(builder);
    }


}
