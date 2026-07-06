#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportSystem.Data;
using AirportSystem.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSystem.Controllers
{
    public class BoardController : Controller
    {
        private readonly AirportContext _db;
        private readonly WeatherService _weather;

        public BoardController(AirportContext db, WeatherService w)
        {
            _db = db;
            _weather = w;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Получаем все рейсы, отсортированные по времени
            var flights = await _db.Flights.OrderBy(f => f.ScheduledDeparture).ToListAsync();

            // 2. Получаем состояние всех взлетных полос
            var runways = await _db.Runways.ToListAsync();

            // 3. Собираем карту погоды для уникальных направлений
            var weatherMap = new Dictionary<string, (string icon, string temp, bool ice)>();
            foreach (var f in flights)
            {
                if (!weatherMap.ContainsKey(f.Destination))
                {
                    var data = await _weather.GetWeatherAsync(f.Destination);
                    weatherMap.Add(f.Destination, (data.Icon, data.Temp, data.DeIcingRequired));
                }
            }

            ViewBag.Weather = weatherMap;
            ViewBag.Runways = runways;

            return View(flights);
        }
    }
}