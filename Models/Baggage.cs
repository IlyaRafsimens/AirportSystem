#nullable disable
namespace AirportSystem.Models
{
    public class Baggage
    {
        public int Id { get; set; }
        public string TagNumber { get; set; }
        public double Weight { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public bool IsOverweightPaid { get; set; }
    }
}