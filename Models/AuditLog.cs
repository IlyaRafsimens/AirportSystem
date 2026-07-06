#nullable disable
using System;

namespace AirportSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string UserEmail { get; set; }
        public string Action { get; set; } // Например: "Изменил статус рейса SU-100"
        public string Role { get; set; }
    }
}