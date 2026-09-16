using Pgvector;

namespace RagCrm.Api.Models;

public class DocumentChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    
    // This stores the AI embedding array (e.g., 1536 dimensions for OpenAI's ada-002)
    public Vector? Embedding { get; set; }
    
    // Navigation property
    public Document? Document { get; set; }
}
