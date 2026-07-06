#nullable disable
using System;
namespace AirportSystem.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; }
        public string Destination { get; set; }
        public string Airline { get; set; }
        public string AircraftType { get; set; }
        public DateTime ScheduledDeparture { get; set; }
        public FlightStatus Status { get; set; }
        public string Gate { get; set; }
        public decimal BasePrice { get; set; }
        public int MaxSeats { get; set; }
    }
}