using Pgvector;

namespace RagCrm.Api.Services;

public class MockEmbeddingService : IEmbeddingService
{
    public Task<Vector> GenerateEmbeddingAsync(string text)
    {
        // Simulates an AI generating a 1536-dimensional vector (matching text-embedding-ada-002)
        var random = new Random();
        var fakeVector = Enumerable.Range(0, 1536).Select(_ => (float)random.NextDouble()).ToArray();
        
        return Task.FromResult(new Vector(fakeVector));
    }
}
