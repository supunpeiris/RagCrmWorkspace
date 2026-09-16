using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Pgvector;

namespace RagCrm.Api.Services;

public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAiEmbeddingService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["OpenAI:ApiKey"] ?? throw new ArgumentException("OpenAI API Key is missing");
    }

    public async Task<Vector> GenerateEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            input = text,
            model = "text-embedding-ada-002" 
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        
        // Extract the 1536 numbers from the JSON response
        var embeddingArray = document.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(e => e.GetSingle())
            .ToArray();

        return new Vector(embeddingArray);
    }
}
