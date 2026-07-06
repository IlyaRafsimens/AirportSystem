#nullable disable
using AirportSystem.Data;
using AirportSystem.Models;
using AirportSystem.Services;
using AirportSystem.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 1. ¡ƒ
builder.Services.AddDbContext<AirportContext>(opt => opt.UseSqlite("Data Source=Airport.db"));

// 2. Identity
builder.Services.AddDefaultIdentity<IdentityUser>(opt => {
    opt.Password.RequiredLength = 4;
    opt.Password.RequireDigit = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireLowercase = false;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AirportContext>();

// 3. ÀŒ ¿À»«¿÷»ﬂ
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddScoped<AirportService>();
builder.Services.AddScoped<SecurityService>();
builder.Services.AddScoped<WeatherService>();

builder.Services.AddControllersWithViews().AddViewLocalization().AddDataAnnotationsLocalization();

var app = builder.Build();

// Õ¿—“–Œ… ¿ œŒƒƒ≈–∆»¬¿≈Ã€’ ﬂ«€ Œ¬
var supportedCultures = new[] { new CultureInfo("ru"), new CultureInfo("en") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ru"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// —»ƒ ƒ¿ÕÕ€’ (20 –≈…—Œ¬ + œŒÀ‹«Œ¬¿“≈À»)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AirportContext>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    db.Database.EnsureCreated();

    string[] roles = { "Admin", "Dispatcher", "User" };
    foreach (var r in roles) if (!roleMgr.RoleExistsAsync(r).Result) roleMgr.CreateAsync(new IdentityRole(r)).Wait();

    async Task EnsureUser(string email, string pass, string role)
    {
        if (await userMgr.FindByEmailAsync(email) == null)
        {
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            await userMgr.CreateAsync(user, pass);
            await userMgr.AddToRoleAsync(user, role);
        }
    }
    EnsureUser("root@airport.ru", "Admin777", "Admin").Wait();
    EnsureUser("disp@airport.ru", "Control555", "Dispatcher").Wait();
    EnsureUser("user@airport.ru", "Pass123", "User").Wait();

    if (!db.Flights.Any())
    {
        db.Flights.AddRange(new List<Flight> {
            new Flight { FlightNumber = "B2-737", Destination = "Minsk", Airline = "Belavia", BasePrice = 120.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(2), Gate = "A1", MaxSeats = 180 },
            new Flight { FlightNumber = "EK-131", Destination = "Dubai", Airline = "Emirates", BasePrice = 1450.50m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(5), Gate = "B1", MaxSeats = 350 },
            new Flight { FlightNumber = "SU-012", Destination = "Moscow", Airline = "Aeroflot", BasePrice = 320.00m, Status = FlightStatus.Boarding, ScheduledDeparture = DateTime.Now.AddMinutes(40), Gate = "C2", MaxSeats = 150 },
            new Flight { FlightNumber = "TK-412", Destination = "Istanbul", Airline = "Turkish", BasePrice = 980.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(8), Gate = "D3", MaxSeats = 200 },
            new Flight { FlightNumber = "AF-114", Destination = "Paris", Airline = "Air France", BasePrice = 1150.00m, Status = FlightStatus.Delayed, ScheduledDeparture = DateTime.Now.AddHours(4), Gate = "E1", MaxSeats = 250 },
            new Flight { FlightNumber = "LH-221", Destination = "Frankfurt", Airline = "Lufthansa", BasePrice = 850.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(12), Gate = "F5", MaxSeats = 190 },
            new Flight { FlightNumber = "KC-911", Destination = "Astana", Airline = "Air Astana", BasePrice = 540.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(6), Gate = "A5", MaxSeats = 160 },
            new Flight { FlightNumber = "WY-121", Destination = "Muscat", Airline = "Oman Air", BasePrice = 1300.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(14), Gate = "B10", MaxSeats = 280 },
            new Flight { FlightNumber = "LO-332", Destination = "Warsaw", Airline = "LOT", BasePrice = 320.00m, Status = FlightStatus.Cancelled, ScheduledDeparture = DateTime.Now.AddHours(3), Gate = "C1", MaxSeats = 120 },
            new Flight { FlightNumber = "EY-065", Destination = "Abu-Dhabi", Airline = "Etihad", BasePrice = 1550.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(20), Gate = "D10", MaxSeats = 300 },
            new Flight { FlightNumber = "FZ-721", Destination = "Tbilisi", Airline = "FlyDubai", BasePrice = 480.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(9), Gate = "A3", MaxSeats = 170 },
            new Flight { FlightNumber = "SU-210", Destination = "Norilsk", Airline = "Aeroflot", BasePrice = 410.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(24), Gate = "B2", MaxSeats = 90 },
            new Flight { FlightNumber = "JU-650", Destination = "Belgrade", Airline = "Air Serbia", BasePrice = 720.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(13), Gate = "D1", MaxSeats = 180 },
            new Flight { FlightNumber = "SQ-321", Destination = "Singapore", Airline = "Singapore", BasePrice = 2900.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(18), Gate = "B12", MaxSeats = 380 },
            new Flight { FlightNumber = "B2-990", Destination = "Tashkent", Airline = "Belavia", BasePrice = 450.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(7), Gate = "A8", MaxSeats = 160 },
            new Flight { FlightNumber = "S7-112", Destination = "Novosibirsk", Airline = "S7", BasePrice = 520.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(10), Gate = "C8", MaxSeats = 160 },
            new Flight { FlightNumber = "EK-202", Destination = "New York", Airline = "Emirates", BasePrice = 3100.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(22), Gate = "B15", MaxSeats = 500 },
            new Flight { FlightNumber = "SU-601", Destination = "St. Petersburg", Airline = "Aeroflot", BasePrice = 190.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddMinutes(120), Gate = "A10", MaxSeats = 150 },
            new Flight { FlightNumber = "SU-302", Destination = "London", Airline = "Aeroflot", BasePrice = 1100.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(11), Gate = "C4", MaxSeats = 250 },
            new Flight { FlightNumber = "TK-413", Destination = "Antalya", Airline = "Turkish", BasePrice = 750.00m, Status = FlightStatus.Scheduled, ScheduledDeparture = DateTime.Now.AddHours(6), Gate = "D2", MaxSeats = 180 }
        });
        db.SaveChanges();

        db.Runways.AddRange(new List<Runway> {
            new Runway { Name = "RWY 01L", Status = RunwayStatus.Free },
            new Runway { Name = "RWY 01R", Status = RunwayStatus.Occupied, CurrentFlight = "EK-131" },
            new Runway { Name = "RWY 02C", Status = RunwayStatus.Maintenance }
        });
        db.SaveChanges();
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<AirportHub>("/airportHub");
app.Run();