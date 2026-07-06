#nullable disable
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AirportSystem.Services
{
    public class WeatherService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        public WeatherService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public async Task<(string Icon, string Temp, bool DeIcingRequired)> GetWeatherAsync(string city)
        {
            if (string.IsNullOrEmpty(city)) return ("☀️", "20°C", false);
            string cacheKey = $"weather_{city.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out (string, string, bool) cached)) return cached;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync($"https://wttr.in/{city}?format=j1");
                using var doc = JsonDocument.Parse(response);
                var current = doc.RootElement.GetProperty("current_condition")[0];
                string temp = current.GetProperty("temp_C").GetString() + "°C";
                bool deIce = int.Parse(current.GetProperty("temp_C").GetString()) <= 0;
                var result = ("☁️", temp, deIce);
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(20));
                return result;
            }
            catch { return ("⛅", "N/A", false); }
        }
    }
}