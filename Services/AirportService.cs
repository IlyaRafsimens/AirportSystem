#nullable disable
using AirportSystem.Data;
using AirportSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSystem.Services
{
    public class AirportService
    {
        private readonly AirportContext _db;
        public AirportService(AirportContext db) => _db = db;

        public async Task<Ticket> GetTicketByNumber(string ticketNumber)
        {
            return await _db.Tickets
                .Include(t => t.Flight)
                .Include(t => t.Passenger)
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);
        }

        // Расчет багажа с учетом лимитов классов
        public (bool isOverweight, decimal fee) CalculateBaggage(TicketClass tClass, double weight)
        {
            double limit = (tClass == TicketClass.Business) ? 30.0 : 23.0;
            bool isOverweight = weight > limit;
            decimal fee = isOverweight ? (decimal)(weight - limit) * 1000 : 0;
            return (isOverweight, fee);
        }

        // Для обратной совместимости
        public (bool isOverweight, decimal fee) CalculateBaggage(double weight, int ticketId)
            => CalculateBaggage(TicketClass.Economy, weight);

        public bool IsRegistrationOpen(DateTime departureTime)
        {
            var timeBeforeFlight = departureTime - DateTime.Now;
            return timeBeforeFlight.TotalHours <= 24 && timeBeforeFlight.TotalHours > 0;
        }

        public bool IsGateAvailable(string gate, int flightId)
        {
            // Гейт свободен, если на нем нет других рейсов в статусе "Посадка"
            return !_db.Flights.Any(f => f.Gate == gate && f.Status == FlightStatus.Boarding && f.Id != flightId);
        }

        // ПОЛНАЯ РЕГИСТРАЦИЯ ПАССАЖИРА И БАГАЖА
        public async Task<string> RegisterPassenger(int ticketId, string seat, double weight)
        {
            var ticket = await _db.Tickets.Include(t => t.Flight).FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return null;

            ticket.SeatNumber = seat;
            ticket.Status = TicketStatus.Registered;
            ticket.RegistrationNumber = "REG-" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper();

            if (weight > 0)
            {
                var baggage = new Baggage
                {
                    TicketId = ticketId,
                    Weight = weight,
                    TagNumber = "TAG-" + ticket.TicketNumber,
                    IsOverweightPaid = CalculateBaggage(ticket.Class, weight).isOverweight
                };
                _db.Baggages.Add(baggage);
            }

            await _db.SaveChangesAsync();
            return ticket.RegistrationNumber;
        }
    }
}