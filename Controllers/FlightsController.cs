#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportSystem.Data;
using AirportSystem.Models;
using AirportSystem.Services;
using AirportSystem.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSystem.Controllers
{
    [Authorize(Roles = "Admin,Dispatcher")]
    public class FlightsController : Controller
    {
        private readonly AirportContext _db;
        private readonly IHubContext<AirportHub> _hub;

        public FlightsController(AirportContext db, IHubContext<AirportHub> hub)
        {
            _db = db; _hub = hub;
        }

        public async Task<IActionResult> Index() => View(await _db.Flights.OrderBy(f => f.ScheduledDeparture).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Flight f)
        {
            _db.Add(f); await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("ReceiveUpdate");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id) => View(await _db.Flights.FindAsync(id));

        [HttpPost]
        public async Task<IActionResult> Edit(Flight f)
        {
            _db.Update(f); await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("ReceiveUpdate");
            return RedirectToAction(nameof(Index));
        }

        // --- УПРАВЛЕНИЕ ПОЛОСАМИ ---
        public async Task<IActionResult> Runway()
        {
            // Передаем список полос И список рейсов для выпадающего списка
            ViewBag.Flights = await _db.Flights.Where(f => f.Status != FlightStatus.Departed).ToListAsync();
            return View(await _db.Runways.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRunway(int id, RunwayStatus status, string flightNum)
        {
            var r = await _db.Runways.FindAsync(id);
            if (r != null)
            {
                r.Status = status;
                r.CurrentFlight = (status == RunwayStatus.Occupied) ? flightNum : "";
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Runway));
        }
    }
}