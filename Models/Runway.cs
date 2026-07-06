#nullable disable
namespace AirportSystem.Models
{
    public enum RunwayStatus { Free, Occupied, Maintenance }

    public class Runway
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RunwayStatus Status { get; set; }
        public string CurrentFlight { get; set; } // Какой рейс сейчас на полосе
    }
}