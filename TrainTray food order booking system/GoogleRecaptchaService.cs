using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class GoogleRecaptchaService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public GoogleRecaptchaService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<bool> VerifyRecaptchaAsync(string recaptchaResponse)
    {
        var secretKey = _configuration["GoogleReCaptcha:SecretKey"];
        var response = await _httpClient.PostAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaResponse}",
            null);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RecaptchaResponse>(json);
        return result.Success;
    }   
}

public class RecaptchaResponse
{
    public bool Success { get; set; }
    public string ChallengeTs { get; set; }
    public string Hostname { get; set; }
}
