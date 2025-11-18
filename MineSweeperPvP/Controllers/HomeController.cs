using Microsoft.AspNetCore.Mvc;

namespace MineSweeperPvP.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");

            ViewBag.IsLoggedIn = userId.HasValue;
            ViewBag.Username = username;

            return View();
        }

        public IActionResult Main()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
    }
}