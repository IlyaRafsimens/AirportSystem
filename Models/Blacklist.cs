#nullable disable
using System;
namespace AirportSystem.Models
{
    public class Blacklist
    {
        public int Id { get; set; }
        public string PassportNumber { get; set; }
        public string FullName { get; set; }
        public string Reason { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}