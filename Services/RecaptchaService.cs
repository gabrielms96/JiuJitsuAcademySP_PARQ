using System.Text.Json.Serialization;

namespace JiuJitsuAcademy.Services;

public interface IRecaptchaService
{
    bool Enabled { get; }
    string SiteKey { get; }
    Task<bool> ValidarAsync(string? token, CancellationToken cancellationToken = default);
}

public class RecaptchaService : IRecaptchaService
{
    private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

    private readonly HttpClient _httpClient;
    private readonly ILogger<RecaptchaService> _logger;
    private readonly bool _enabled;
    private readonly string _siteKey;
    private readonly string _secretKey;
    private readonly double _minimumScore;

    public RecaptchaService(HttpClient httpClient, IConfiguration configuration, ILogger<RecaptchaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var section = configuration.GetSection("Recaptcha");
        _enabled = section.GetValue("Enabled", false);
        _siteKey = section.GetValue("SiteKey", string.Empty) ?? string.Empty;
        _secretKey = section.GetValue("SecretKey", string.Empty) ?? string.Empty;
        _minimumScore = section.GetValue("MinimumScore", 0.5);
    }

    public bool Enabled => _enabled && !string.IsNullOrWhiteSpace(_siteKey) && !string.IsNullOrWhiteSpace(_secretKey);

    public string SiteKey => _siteKey;

    public async Task<bool> ValidarAsync(string? token, CancellationToken cancellationToken = default)
    {
        // Se o reCAPTCHA estiver desabilitado (ex.: ambiente local sem chaves), libera.
        if (!Enabled)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsync(
                $"{VerifyUrl}?secret={Uri.EscapeDataString(_secretKey)}&response={Uri.EscapeDataString(token)}",
                content: null,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>(cancellationToken);
            if (result is null)
            {
                return false;
            }

            // reCAPTCHA v3 retorna score; v2 nao. Validamos ambos.
            var aprovado = result.Success && (result.Score is null || result.Score >= _minimumScore);

            if (!aprovado)
            {
                _logger.LogWarning("reCAPTCHA reprovado. Success={Success} Score={Score}.",
                    result.Success, result.Score);
            }

            return aprovado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao validar reCAPTCHA.");
            return false;
        }
    }

    private sealed class RecaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("score")]
        public double? Score { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
