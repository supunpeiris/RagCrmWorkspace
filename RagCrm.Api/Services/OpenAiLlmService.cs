using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RagCrm.Api.Services;

public class OpenAiLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAiLlmService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["OpenAI:ApiKey"] ?? throw new ArgumentException("OpenAI API Key is missing");
    }

    public async Task<string> GenerateAnswerAsync(string prompt, List<string> contextChunks)
    {
        // 1. Build the strict RAG instructions
        var systemMessage = "You are an intelligent CRM assistant. Answer the user's question using ONLY the context provided below. If the answer is not contained in the context, explicitly state: 'I cannot answer this based on the provided documents.' Do not use outside knowledge.\n\nContext:\n" 
                            + string.Join("\n---\n", contextChunks);

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = systemMessage },
                new { role = "user", content = prompt }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        
        // Extract the AI's final text answer
        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "No response generated.";
    }
}
