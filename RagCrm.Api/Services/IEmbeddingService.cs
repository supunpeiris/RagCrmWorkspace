using Pgvector;

namespace RagCrm.Api.Services;

public interface IEmbeddingService
{
    // A single interface handles both document chunk embedding and search query embedding
    Task<Vector> GenerateEmbeddingAsync(string text);
}
