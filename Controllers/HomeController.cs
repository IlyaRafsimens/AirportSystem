#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportSystem.Data;
using AirportSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AirportContext _context;
        public HomeController(AirportContext context) => _context = context;

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("MyProfile");
            return View();
        }

        public async Task<IActionResult> MyProfile()
        {
            var user = User;

            if (user.IsInRole("Admin"))
            {
                // Данные для Админа
                var tickets = await _context.Tickets.ToListAsync();
                ViewBag.TotalRevenue = tickets.Where(t => t.Status != TicketStatus.Cancelled).Sum(t => t.FinalPrice);
                ViewBag.TotalUsers = await _context.Users.CountAsync();
                ViewBag.BannedCount = await _context.Blacklists.CountAsync();
                return View("MyProfileAdmin"); // Используем отдельное представление для чистоты
            }

            if (user.IsInRole("Dispatcher"))
            {
                // Данные для Диспетчера
                ViewBag.ActiveFlights = await _context.Flights.CountAsync(f => f.Status == FlightStatus.Boarding || f.Status == FlightStatus.Scheduled);
                ViewBag.BusyRunways = await _context.Runways.CountAsync(r => r.Status != RunwayStatus.Free);
                return View("MyProfileDispatcher");
            }

            // Данные для обычного Пользователя
            var myTickets = await _context.Tickets
                .Include(t => t.Flight)
                .Include(t => t.Passenger)
                .Where(t => t.UserId == user.Identity.Name)
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            return View(myTickets);
        }

        public IActionResult Privacy() => View();
    }
}