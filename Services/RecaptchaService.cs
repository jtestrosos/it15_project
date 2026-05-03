using System.Text.Json;
using System.Text.Json.Serialization;

namespace production_system.Services;

public interface IRecaptchaService
{
    Task<bool> VerifyTokenAsync(string token);
}

public class RecaptchaService : IRecaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly ILogger<RecaptchaService> _logger;

    public RecaptchaService(HttpClient httpClient, IConfiguration configuration, ILogger<RecaptchaService> logger)
    {
        _httpClient = httpClient;
        _secretKey = configuration["Recaptcha:SecretKey"] ?? throw new InvalidOperationException("Recaptcha:SecretKey is not configured.");
        _logger = logger;
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("reCAPTCHA token is empty.");
            return false;
        }

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", _secretKey },
                { "response", token }
            });

            var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RecaptchaResponse>(json);

            if (result is null)
            {
                _logger.LogWarning("reCAPTCHA verification returned null response.");
                return false;
            }

            if (!result.Success)
            {
                _logger.LogWarning("reCAPTCHA verification failed. Error codes: {Errors}", 
                    result.ErrorCodes != null ? string.Join(", ", result.ErrorCodes) : "none");
            }

            return result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying reCAPTCHA token.");
            return false;
        }
    }

    private class RecaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("challenge_ts")]
        public string? ChallengeTimestamp { get; set; }

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
