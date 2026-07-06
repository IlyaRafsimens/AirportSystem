#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportSystem.Data;
using AirportSystem.Models;
using AirportSystem.Services;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly AirportContext _db;
        private readonly SecurityService _sec;
        private readonly WeatherService _weather;

        public BookingController(AirportContext context, SecurityService security, WeatherService weather)
        {
            _db = context; _sec = security; _weather = weather;
        }

        // Список рейсов
        public async Task<IActionResult> Index()
        {
            var flights = await _db.Flights.Where(f => f.Status == FlightStatus.Scheduled).ToListAsync();
            var occupancyMap = new Dictionary<int, int>();
            foreach (var f in flights)
            {
                int taken = await _db.Tickets.CountAsync(t => t.FlightId == f.Id && t.Status != TicketStatus.Cancelled);
                occupancyMap.Add(f.Id, taken);
            }
            var wMap = new Dictionary<string, (string icon, string temp, bool ice)>();
            foreach (var f in flights)
            {
                if (!wMap.ContainsKey(f.Destination))
                {
                    var w = await _weather.GetWeatherAsync(f.Destination);
                    wMap.Add(f.Destination, (w.Icon, w.Temp, w.DeIcingRequired));
                }
            }
            ViewBag.Weather = wMap;
            ViewBag.Occupancy = occupancyMap;
            return View(flights);
        }

        public async Task<IActionResult> Checkout(int flightId)
        {
            var flight = await _db.Flights.FindAsync(flightId);
            ViewBag.Flight = flight;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Finalize(int flightId, string fullName, string passport, TicketClass tClass)
        {
            var flight = await _db.Flights.FindAsync(flightId);
            var passenger = await _db.Passengers.FirstOrDefaultAsync(p => p.PassportNumber == passport) ?? new Passenger { FullName = fullName.ToUpper(), PassportNumber = passport };
            if (passenger.Id == 0) { _db.Passengers.Add(passenger); await _db.SaveChangesAsync(); }

            var ticket = new Ticket
            {
                TicketNumber = "BY-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                FlightId = flightId,
                PassengerId = passenger.Id,
                Class = tClass,
                Status = TicketStatus.Paid,
                FinalPrice = flight.BasePrice + (tClass == TicketClass.Business ? 450.00m : 0),
                UserId = User.Identity.Name,
                SeatNumber = new Random().Next(1, 30) + "A"
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
            return RedirectToAction("Success", new { id = ticket.Id });
        }

        public async Task<IActionResult> Success(int id)
        {
            var t = await _db.Tickets.Include(x => x.Flight).Include(x => x.Passenger).FirstOrDefaultAsync(x => x.Id == id);
            using (QRCodeGenerator qr = new QRCodeGenerator())
            {
                var data = qr.CreateQrCode(t.TicketNumber, QRCodeGenerator.ECCLevel.Q);
                var code = new PngByteQRCode(data);
                ViewBag.QrCode = "data:image/png;base64," + Convert.ToBase64String(code.GetGraphic(20));
            }
            return View(t);
        }

        // ЭТОТ МЕТОД ИСПРАВЛЯЕТ ОШИБКУ 404
        public async Task<IActionResult> Print(int id)
        {
            var t = await _db.Tickets.Include(x => x.Flight).Include(x => x.Passenger).FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return NotFound();

            using (QRCodeGenerator qr = new QRCodeGenerator())
            {
                var data = qr.CreateQrCode(t.TicketNumber, QRCodeGenerator.ECCLevel.Q);
                var code = new PngByteQRCode(data);
                ViewBag.QrCode = "data:image/png;base64," + Convert.ToBase64String(code.GetGraphic(20));
            }
            return View(t);
        }

        [HttpPost]
        public async Task<IActionResult> CancelTicket(int id)
        {
            var t = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == id && x.UserId == User.Identity.Name);
            if (t != null) { t.Status = TicketStatus.Cancelled; await _db.SaveChangesAsync(); }
            return RedirectToAction("MyProfile", "Home");
        }
    }
}