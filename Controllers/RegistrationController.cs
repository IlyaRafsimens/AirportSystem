#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportSystem.Data;
using AirportSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSystem.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly AirportContext _db;
        public RegistrationController(AirportContext db) => _db = db;

        // Поиск билета для регистрации
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Search(string ticketNumber)
        {
            var t = await _db.Tickets.Include(x => x.Flight).FirstOrDefaultAsync(x => x.TicketNumber == ticketNumber);
            if (t == null) { ViewBag.Error = "Билет не найден"; return View("Index"); }

            // Собираем занятые места на этом рейсе
            ViewBag.OccupiedSeats = await _db.Tickets.Where(x => x.FlightId == t.FlightId && x.SeatNumber != null).Select(x => x.SeatNumber).ToListAsync();
            return View("CheckIn", t);
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int ticketId, string seat, double weight)
        {
            var t = await _db.Tickets.Include(x => x.Flight).FirstOrDefaultAsync(x => x.Id == ticketId);
            t.SeatNumber = seat;
            t.Status = TicketStatus.Registered;
            t.RegistrationNumber = "BY-REG-" + t.Id + "-" + new System.Random().Next(100, 999);

            if (weight > 0)
            {
                _db.Baggages.Add(new Baggage { TicketId = ticketId, Weight = weight, TagNumber = "LUG-" + t.TicketNumber, IsOverweightPaid = weight > 23 });
            }

            _db.AuditLogs.Add(new AuditLog { UserEmail = User.Identity.Name, Role = "User", Action = $"Пассажир зарегистрирован на рейс {t.Flight.FlightNumber}, место {seat}" });
            await _db.SaveChangesAsync();
            return RedirectToAction("Success", "Booking", new { id = ticketId });
        }
    }
}