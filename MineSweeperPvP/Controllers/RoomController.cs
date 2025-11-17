using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MineSweeperPvP.Datas;
using MineSweeperPvP.Models;

namespace MineSweeperPvP.Controllers
{
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _db;
        public RoomController(ApplicationDbContext db) { _db = db; }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> CreateRoom()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var roomId = GenerateRoomId();
            var room = new Room { RoomId = roomId, Player1Id = userId.Value, CreateDay = DateTime.UtcNow };
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return RedirectToAction("Room", new { id = roomId });
        }

        [HttpGet]
        public IActionResult Join() => View();

        [HttpPost]
        public async Task<IActionResult> JoinRoom(string roomId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (room == null) { ModelState.AddModelError("", "Room not found"); return View("Join"); }
            if (room.Player2Id == null)
            {
                room.Player2Id = userId.Value;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Room", new { id = roomId });
        }

        public async Task<IActionResult> Room(string id)
        {
            // show room with 2 grids and join via SignalR
            var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
            if (room == null) return NotFound();
            ViewBag.RoomId = id;
            // determine if current user is p1 or p2
            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.PlayerNumber = (userId == room.Player1Id) ? 1 : 2;
            return View();
        }

        private string GenerateRoomId()
        {
            // simple random ID (6 chars)
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rnd = new Random();
            return new string(Enumerable.Range(0, 6).Select(i => chars[rnd.Next(chars.Length)]).ToArray());
        }
    }
}
