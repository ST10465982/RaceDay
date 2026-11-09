using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetInt32("UserID") is not null)
        {
            return HttpContext.Session.GetString("Role") == "Organiser"
                ? RedirectToAction("Index", "Dashboard")
                : RedirectToAction("Explore", "Events");
        }
        return View();
    }

    public IActionResult Error() => View();
}
