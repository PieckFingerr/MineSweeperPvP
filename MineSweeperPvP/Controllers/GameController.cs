using Microsoft.AspNetCore.Mvc;
using MineSweeperPvP.Datas;

namespace MineSweeperPvP.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _db;
        public GameController(ApplicationDbContext db) { _db = db; }

        [HttpGet]
        public IActionResult Play(string difficulty = "Medium")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Difficulty = difficulty;
            return View();
        }
    }
}
