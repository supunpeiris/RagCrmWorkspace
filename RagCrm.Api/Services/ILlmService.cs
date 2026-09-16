namespace RagCrm.Api.Services;

public interface ILlmService
{
    // Takes the user's question and the raw text chunks found in the database
    Task<string> GenerateAnswerAsync(string prompt, List<string> contextChunks);
}
