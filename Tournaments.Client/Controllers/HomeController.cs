using Microsoft.AspNetCore.Mvc;

namespace Tournaments.Client.Controllers;
public class HomeController : Controller
{
    private HttpClient HttpClient;

    public HomeController()
    {
        HttpClient = new HttpClient();
    }


    public IActionResult Index()
    {
        return View();
    }
}
