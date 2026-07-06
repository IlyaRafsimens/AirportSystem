#nullable disable
namespace AirportSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public string RegistrationNumber { get; set; } // НОВЫЙ НОМЕР РЕГИСТРАЦИИ
        public int FlightId { get; set; }
        public virtual Flight Flight { get; set; }
        public int PassengerId { get; set; }
        public virtual Passenger Passenger { get; set; }
        public string UserId { get; set; }
        public TicketClass Class { get; set; }
        public string SeatNumber { get; set; }
        public TicketStatus Status { get; set; }
        public decimal FinalPrice { get; set; }
    }
}