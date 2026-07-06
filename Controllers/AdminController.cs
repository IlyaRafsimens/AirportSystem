#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportSystem.Data;
using AirportSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AirportContext _context;

        public AdminController(AirportContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var flights = await _context.Flights.ToListAsync();
            var tickets = await _context.Tickets.ToListAsync();

            var loadData = new Dictionary<string, double>();
            foreach (var f in flights)
            {
                int sold = tickets.Count(t => t.FlightId == f.Id && t.Status != TicketStatus.Cancelled);
                loadData.Add(f.FlightNumber, f.MaxSeats > 0 ? (double)sold / f.MaxSeats * 100 : 0);
            }

            ViewBag.LoadData = loadData;
            ViewBag.TotalPassengers = tickets.Count(t => t.Status != TicketStatus.Cancelled);
            ViewBag.TotalFlights = flights.Count;
            ViewBag.DelayedCount = flights.Count(f => f.Status == FlightStatus.Delayed);
            ViewBag.TotalRevenue = tickets.Where(t => t.Status != TicketStatus.Cancelled).Sum(t => t.FinalPrice);

            return View();
        }

        // ЖУРНАЛ АУДИТА
        public async Task<IActionResult> Audit()
        {
            var logs = await _context.AuditLogs.OrderByDescending(x => x.Timestamp).ToListAsync();
            return View(logs);
        }

        // ФИНАНСОВЫЙ ОТЧЕТ
        public async Task<IActionResult> Report()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Flight)
                .Include(t => t.Passenger)
                .Where(t => t.Status != TicketStatus.Cancelled)
                .ToListAsync();

            ViewBag.Total = tickets.Sum(t => t.FinalPrice);
            return View(tickets);
        }

        public async Task<IActionResult> Tickets() => View(await _context.Tickets.Include(t => t.Flight).Include(t => t.Passenger).ToListAsync());
        public async Task<IActionResult> Baggage() => View(await _context.Baggages.Include(b => b.Ticket).ThenInclude(t => t.Passenger).ToListAsync());
        public async Task<IActionResult> Blacklist() => View(await _context.Blacklists.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> AddToBlacklist(string passport, string name, string reason)
        {
            _context.Blacklists.Add(new Blacklist { PassportNumber = passport, FullName = name.ToUpper(), Reason = reason });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Blacklist));
        }

        public async Task<IActionResult> RemoveFromBlacklist(int id)
        {
            var entry = await _context.Blacklists.FindAsync(id);
            if (entry != null) { _context.Blacklists.Remove(entry); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Blacklist));
        }
    }
}