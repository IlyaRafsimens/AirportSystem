#nullable disable
namespace AirportSystem.Models
{
    public class Passenger
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PassportNumber { get; set; }
        public string Citizenship { get; set; }
        public DateTime BirthDate { get; set; }
    }
}