using System.Text.Json;
using System.Text.Json.Serialization;

namespace manufacturing_system.Services
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateData?> GetExchangeRatesAsync(string apiKey, string baseCurrency = "USD");
        Task<decimal> ConvertToPhpAsync(decimal amount, string apiKey, string fromCurrency = "USD");
    }

    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly ILogger<ExchangeRateService> _logger;

        // Cache rates for 1 hour to avoid excessive API calls
        private ExchangeRateData? _cachedRates;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private string? _cachedApiKey;

        public ExchangeRateService(HttpClient httpClient, IConfiguration configuration, ILogger<ExchangeRateService> logger)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:ExchangeRateBaseUrl"] ?? "https://v6.exchangerate-api.com/v6";
            _logger = logger;
        }

        public async Task<ExchangeRateData?> GetExchangeRatesAsync(string apiKey, string baseCurrency = "USD")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("Exchange Rate API key is not provided.");
                    return null;
                }

                // Return cached data if still valid
                if (_cachedRates != null && DateTime.UtcNow < _cacheExpiry && _cachedRates.BaseCode == baseCurrency && _cachedApiKey == apiKey)
                {
                    return _cachedRates;
                }

                var url = $"{_baseUrl}/{apiKey}/latest/{baseCurrency}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ExchangeRate API returned {StatusCode} for base {Currency}",
                        response.StatusCode, baseCurrency);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ExchangeRateApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data == null || data.Result != "success") return null;

                _cachedRates = new ExchangeRateData
                {
                    BaseCode = data.BaseCode ?? baseCurrency,
                    PhpRate = data.ConversionRates?.GetValueOrDefault("PHP") ?? 0,
                    UsdRate = data.ConversionRates?.GetValueOrDefault("USD") ?? 1,
                    EurRate = data.ConversionRates?.GetValueOrDefault("EUR") ?? 0,
                    JpyRate = data.ConversionRates?.GetValueOrDefault("JPY") ?? 0,
                    CnyRate = data.ConversionRates?.GetValueOrDefault("CNY") ?? 0,
                    AllRates = data.ConversionRates ?? new Dictionary<string, decimal>(),
                    LastUpdated = DateTime.UtcNow
                };

                _cachedApiKey = apiKey;
                _cacheExpiry = DateTime.UtcNow.AddHours(1);

                return _cachedRates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rates for {Currency}", baseCurrency);
                return null;
            }
        }

        public async Task<decimal> ConvertToPhpAsync(decimal amount, string apiKey, string fromCurrency = "USD")
        {
            var rates = await GetExchangeRatesAsync(apiKey, fromCurrency);
            if (rates == null) return amount; // Return unconverted if API fails

            return amount * rates.PhpRate;
        }
    }

    public class ExchangeRateData
    {
        public string BaseCode { get; set; } = "USD";
        public decimal PhpRate { get; set; }
        public decimal UsdRate { get; set; }
        public decimal EurRate { get; set; }
        public decimal JpyRate { get; set; }
        public decimal CnyRate { get; set; }
        public Dictionary<string, decimal> AllRates { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class ExchangeRateApiResponse
    {
        [JsonPropertyName("result")]
        public string? Result { get; set; }

        [JsonPropertyName("base_code")]
        public string? BaseCode { get; set; }

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal>? ConversionRates { get; set; }
    }
}
