namespace RagCrm.Api.Services;

public class MockLlmService : ILlmService
{
    public Task<string> GenerateAnswerAsync(string prompt, List<string> contextChunks)
    {
        // Simulate the delay of an AI processing the text
        var simulatedResponse = $"[Simulated AI Answer] You asked: '{prompt}'. \n\n" +
                                $"Based on the document, I synthesized my answer using {contextChunks.Count} retrieved chunks of data. " +
                                $"Here is the raw information I used to generate this response:\n\n- " +
                                string.Join("\n- ", contextChunks);
        
        return Task.FromResult(simulatedResponse);
    }
}
