using GemBardPT.Controllers;
using GemBardPT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
public class GeminiController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeminiApiSettings _geminiApiSettings;

    public GeminiController(IHttpClientFactory httpClientFactory, IOptions<GeminiApiSettings> geminiApiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _geminiApiSettings = geminiApiSettings.Value;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetResponseFromGemini(string input)
    {
        ViewBag.UserInput = input;

        if (string.IsNullOrWhiteSpace(input) || input.Length > 500)
        {
            return BadRequest("Texto inválido. Certifique-se de que a entrada tenha entre 1 e 500 caracteres.");
        }

        try
        {
            var apiKey = _geminiApiSettings.ApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest("Chave da API do Gemini ausente ou inválida.");
            }

            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta2/models/{_geminiApiSettings.ModelName}:generateContent";
            var requestData = new GeminiApiRequest
            {
                Contents = new[]
                {
                    new GeminiContent
                    {
                        Role = "user",
                        Parts = new[] { new { Text = input } }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    Temperature = 0.7,
                    MaxOutputTokens = 100,
                    TopK = 40,
                    TopP = 0.95,
                    StopSequences = Array.Empty<string>()
                }
            };

            using (var client = _httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                using (var response = await client.PostAsync(apiUrl, content))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        // Log the error for debugging.
                        return StatusCode((int)response.StatusCode, $"Erro na API: {response.StatusCode} - {errorContent}");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<GeminiApiResponse>(responseContent);

                    return View("Response", apiResponse);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, $"Network Error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"JSON Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Log the exception for debugging.
            return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}