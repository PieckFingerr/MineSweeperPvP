using Microsoft.AspNetCore.Mvc;
using MineSweeperPvP.Datas;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using MineSweeperPvP.Models;
using System.Text;

namespace MineSweeperPvP.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AccountController(ApplicationDbContext db) { _db = db; }

        private string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        [HttpGet] public IActionResult Register() => View();
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return View();
            if (await _db.Users.AnyAsync(u => u.Username == username)) { ModelState.AddModelError("", "Username taken"); return View(); }
            var u = new User { Username = username, PasswordHash = Hash(password) };
            _db.Users.Add(u);
            await _db.SaveChangesAsync();
            HttpContext.Session.SetInt32("UserId", u.UserId);
            HttpContext.Session.SetString("Username", u.Username);
            return RedirectToAction("Main", "Home");
        }

        [HttpGet] public IActionResult Login() => View();
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var hash = Hash(password);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hash);
            if (user == null) { ModelState.AddModelError("", "Invalid"); return View(); }
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            return RedirectToAction("Main", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
