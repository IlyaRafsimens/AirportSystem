#nullable disable
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AirportSystem.Models;

namespace AirportSystem.Data
{
    public class AirportContext : IdentityDbContext
    {
        public AirportContext(DbContextOptions<AirportContext> options) : base(options) { }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Baggage> Baggages { get; set; }
        public DbSet<Blacklist> Blacklists { get; set; }
        public DbSet<Runway> Runways { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Ticket>().HasIndex(t => new { t.FlightId, t.PassengerId }).IsUnique();
        }
    }
}