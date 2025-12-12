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

            // ✅ Debug: Kiểm tra userId
            if (userId == null)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            // ✅ Debug: Log để kiểm tra
            Console.WriteLine($"Creating room for UserId: {userId.Value}");

            var roomId = GenerateRoomId();
            var room = new Room
            {
                RoomId = roomId,
                Player1Id = userId.Value,  // ✅ Đảm bảo lấy đúng userId
                Player2Id = null,
                CreateDay = DateTime.UtcNow
            };

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();

            // ✅ Debug: Kiểm tra lại sau khi save
            var savedRoom = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
            Console.WriteLine($"Saved room - Player1Id: {savedRoom?.Player1Id}, Player2Id: {savedRoom?.Player2Id}");

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
            if (room == null)
            {
                ModelState.AddModelError("", "Room not found");
                return View("Join");
            }

            // ✅ Kiểm tra nếu người join là chính người tạo room
            if (room.Player1Id == userId.Value)
            {
                return RedirectToAction("Room", new { id = roomId });
            }

            // ✅ Chỉ gán Player2Id nếu chưa có người join
            if (room.Player2Id == null)
            {
                room.Player2Id = userId.Value;
                await _db.SaveChangesAsync();

                Console.WriteLine($"User {userId.Value} joined room {roomId} as Player2");
            }
            else if (room.Player2Id != userId.Value)
            {
                ModelState.AddModelError("", "Room is full");
                return View("Join");
            }

            return RedirectToAction("Room", new { id = roomId });
        }

        public async Task<IActionResult> Room(string id)
        {
            var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
            if (room == null) return NotFound();

            ViewBag.RoomId = id;

            var userId = HttpContext.Session.GetInt32("UserId");

            // ✅ Debug
            Console.WriteLine($"Room access - UserId: {userId}, Player1Id: {room.Player1Id}, Player2Id: {room.Player2Id}");

            // ✅ Xác định player number chính xác
            if (userId == room.Player1Id)
                ViewBag.PlayerNumber = 1;
            else if (userId == room.Player2Id)
                ViewBag.PlayerNumber = 2;
            else
            {
                TempData["Error"] = "You are not a player in this room";
                return RedirectToAction("Main", "Home");
            }

            return View();
        }

        private string GenerateRoomId()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rnd = new Random();
            return new string(Enumerable.Range(0, 6).Select(i => chars[rnd.Next(chars.Length)]).ToArray());
        }
    }
}