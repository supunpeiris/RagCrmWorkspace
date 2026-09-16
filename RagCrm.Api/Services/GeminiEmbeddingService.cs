using System.Text;
using System.Text.Json;
using Pgvector;

namespace RagCrm.Api.Services;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiEmbeddingService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentException("Gemini API Key is missing");
    }

           public async Task<Vector> GenerateEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            model = "models/gemini-embedding-001",
            content = new { parts = new[] { new { text } } },
            outputDimensionality = 768 // Explicitly request the 768 dimension truncation
        };

        var request = new HttpRequestMessage(HttpMethod.Post, 
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={_apiKey}");
        
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        
        var embeddingArray = document.RootElement
            .GetProperty("embedding")
            .GetProperty("values")
            .EnumerateArray()
            .Select(e => e.GetSingle())
            .ToArray();

        return new Vector(embeddingArray);
    }


}
