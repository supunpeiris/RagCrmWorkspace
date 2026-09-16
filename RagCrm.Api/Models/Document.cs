namespace RagCrm.Api.Models;

public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public List<DocumentChunk> Chunks { get; set; } = new();
}
