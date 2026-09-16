using System.Text;
using System.Text.Json;

namespace RagCrm.Api.Services;

public class GeminiLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiLlmService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentException("Gemini API Key is missing");
    }

    public async Task<string> GenerateAnswerAsync(string prompt, List<string> contextChunks)
    {
        var systemContext = "You are an intelligent CRM assistant. Answer the user's question using ONLY the context provided below.\n\nContext:\n"
                            + string.Join("\n---\n", contextChunks);

        var requestBody = new
        {
            systemInstruction = new { parts = new[] { new { text = systemContext } } },
            contents = new[] { new { parts = new[] { new { text = prompt } } } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, 
    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={_apiKey}");


        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);

        return document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "No response generated.";
    }

}
