// Ignore Spelling: api json Deserialize deserialization

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Tournaments.Shared.Dto;

namespace Tournaments.Client.Controllers;
public class HomeController : Controller
{
    private HttpClient httpClient;

    public HomeController()
    {
        httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7225");
    }

    public async Task<IActionResult> Index()
    {
        IEnumerable<TournamentDto> result = await SimpleGetAsync();
        IEnumerable<TournamentDto> result2 = await SimpleGetAsync();
        return View();
    }

    private async Task<IEnumerable<TournamentDto>> SimpleGetAsync()
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/tournamentDetails");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();//returns a string.

        //Deserialize the string to json and make sure that we get Camel Case for a C# object.
        //Note: the Api returns an ApiBaseResponse thus in this project the
        //"PropertyNamingPolicy = JsonNamingPolicy.CamelCase" is not needed. However, add the 
        //policy to guard the deserialization result.
        IEnumerable<TournamentDto>? tournaments = JsonSerializer
            .Deserialize<IEnumerable<TournamentDto>>(result,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        return tournaments!;
    }

    private async Task<IEnumerable<TournamentDto>> SimpleGetAsync2()
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<TournamentDto>>("api/tournamentDetails",
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
