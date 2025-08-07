// Ignore Spelling: api

using Microsoft.AspNetCore.Mvc;
using Tournaments.Shared.Dto;

namespace Tournaments.Client.Controllers;
public class HomeController : Controller
{
    private HttpClient HttpClient;

    public HomeController()
    {
        HttpClient = new HttpClient();
        HttpClient.BaseAddress = new Uri("https://localhost:7225");
    }


    public async Task<IActionResult> Index()
    {
        var result = await SimpleGetAsync();
        return View();
    }

    private async Task<IEnumerable<TournamentDto>> SimpleGetAsync()
    {
        HttpResponseMessage response = await HttpClient.GetAsync("api/tournamentDetails");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();//returns a string.

        return null;
    }
}
