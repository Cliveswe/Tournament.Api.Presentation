using Microsoft.AspNetCore.Mvc;

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

    private async Task SimpleGetAsync()
    {
        var response = await HttpClient.GetAsync("api/tournaments");
    }
}
