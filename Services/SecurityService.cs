using AirportSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace AirportSystem.Services
{
    public class SecurityService
    {
        private readonly AirportContext _db;
        public SecurityService(AirportContext db) => _db = db;

        // Проверка: находится ли паспорт в черном списке
        public async Task<bool> IsPassengerBanned(string passport)
        {
            if (string.IsNullOrEmpty(passport)) return false;
            return await _db.Blacklists.AnyAsync(b => b.PassportNumber == passport);
        }

        // Получить причину бана
        public async Task<string> GetBanReason(string passport)
        {
            var entry = await _db.Blacklists.FirstOrDefaultAsync(b => b.PassportNumber == passport);
            return entry?.Reason ?? "Причина не указана";
        }
    }
}