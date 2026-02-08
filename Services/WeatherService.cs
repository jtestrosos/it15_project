using System.Text.Json;
using System.Text.Json.Serialization;

namespace manufacturing_system.Services
{
    public interface IWeatherService
    {
        Task<WeatherData?> GetCurrentWeatherAsync(string location);
    }

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ApiSettings:OpenWeatherApiKey"] ?? "";
            _baseUrl = configuration["ApiSettings:OpenWeatherBaseUrl"] ?? "https://api.openweathermap.org/data/2.5";
            _logger = logger;
        }

        public async Task<WeatherData?> GetCurrentWeatherAsync(string location)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(location))
                {
                    location = "Davao City,PH"; // Default location
                }

                var url = $"{_baseUrl}/weather?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric";
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenWeather API returned {StatusCode} for location {Location}", 
                        response.StatusCode, location);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<OpenWeatherResponse>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (data == null) return null;

                return new WeatherData
                {
                    Temperature = data.Main?.Temp ?? 0,
                    Humidity = data.Main?.Humidity ?? 0,
                    Description = data.Weather?.FirstOrDefault()?.Description ?? "Unknown",
                    Icon = data.Weather?.FirstOrDefault()?.Icon ?? "01d",
                    Location = data.Name ?? location,
                    Country = data.Sys?.Country ?? "",
                    FeelsLike = data.Main?.FeelsLike ?? 0,
                    WindSpeed = data.Wind?.Speed ?? 0,
                    Pressure = data.Main?.Pressure ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data for {Location}", location);
                return null;
            }
        }
    }

    public class WeatherData
    {
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Location { get; set; } = "";
        public string Country { get; set; } = "";
        public decimal FeelsLike { get; set; }
        public decimal WindSpeed { get; set; }
        public int Pressure { get; set; }

        public string GetAlertStatus()
        {
            if (Temperature > 40) return "Critical";
            if (Temperature > 35) return "Warning";
            if (Temperature < 10) return "Cold Warning";
            if (Humidity > 80) return "High Humidity";
            return "Normal";
        }

        public string IconUrl => $"https://openweathermap.org/img/wn/{Icon}@2x.png";
    }

    // OpenWeather API response models
    public class OpenWeatherResponse
    {
        public OpenWeatherMain? Main { get; set; }
        public List<OpenWeatherWeather>? Weather { get; set; }
        public OpenWeatherWind? Wind { get; set; }
        public OpenWeatherSys? Sys { get; set; }
        public string? Name { get; set; }
    }

    public class OpenWeatherMain
    {
        public decimal Temp { get; set; }
        
        [JsonPropertyName("feels_like")]
        public decimal FeelsLike { get; set; }
        
        public decimal Humidity { get; set; }
        public int Pressure { get; set; }
    }

    public class OpenWeatherWeather
    {
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }

    public class OpenWeatherWind
    {
        public decimal Speed { get; set; }
    }

    public class OpenWeatherSys
    {
        public string? Country { get; set; }
    }
}
