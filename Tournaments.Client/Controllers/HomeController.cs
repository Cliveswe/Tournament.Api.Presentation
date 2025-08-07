using Microsoft.AspNetCore.Mvc;

namespace Tournaments.Client.Controllers;
public class HomeController : Controller
{

    public IActionResult Index()
    {
        return View();
    }
}
