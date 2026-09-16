using Pgvector.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagCrm.Api.Data;
using RagCrm.Api.Models;
using RagCrm.Api.Services;
using Microsoft.EntityFrameworkCore;


namespace RagCrm.Api.Controllers;

[Authorize] // This protects the endpoint, requiring your JWT token!
[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILlmService _llmService; 

    // Update the constructor
    public DocumentController(
        AppDbContext context,
        IEmbeddingService embeddingService,
        ILlmService llmService)
    {
        _context = context;
        _embeddingService = embeddingService;
        _llmService = llmService;
    }



    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // 1. Read the text from the uploaded file
        using var reader = new StreamReader(file.OpenReadStream());
        var rawText = await reader.ReadToEndAsync();

        // 2. Save the parent Document record
        var document = new Document
        {
            FileName = file.FileName
        };
        _context.Documents.Add(document);

        // 3. Chunk the text (splitting by paragraphs for simplicity)
        var chunks = rawText.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        // 4. Generate embeddings and save chunks
        foreach (var chunkText in chunks)
        {
            if (string.IsNullOrWhiteSpace(chunkText)) continue;

            // Call the AI Service to get the vector representation
            var embeddingVector = await _embeddingService.GenerateEmbeddingAsync(chunkText);

            var documentChunk = new DocumentChunk
            {
                DocumentId = document.Id,
                Content = chunkText.Trim(),
                Embedding = embeddingVector
            };

            _context.DocumentChunks.Add(documentChunk);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Document processed, chunked, and embedded successfully.",
            DocumentId = document.Id,
            TotalChunks = chunks.Length
        });
    }

        [HttpPost("search")]
    public async Task<IActionResult> SearchDocuments([FromBody] SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt cannot be empty.");

        // 1. Convert the user's question into a mathematical vector
        var queryVector = await _embeddingService.GenerateEmbeddingAsync(request.Prompt);

        // 2. Perform the Vector Similarity Search in PostgreSQL
        var relevantChunks = await _context.DocumentChunks
            .OrderBy(c => c.Embedding!.CosineDistance(queryVector))
            .Take(3)
            .Select(c => new 
            {
                DocumentName = c.Document!.FileName,
                ExtractedText = c.Content
            })
            .ToListAsync();

        // 3. Extract just the text to feed to the LLM
        var contextTexts = relevantChunks.Select(c => c.ExtractedText).ToList();

        // 4. Generate the final AI answer!
        var aiAnswer = await _llmService.GenerateAnswerAsync(request.Prompt, contextTexts);

        // 5. Return the final structured response to the frontend
        return Ok(new 
        { 
            Question = request.Prompt,
            Answer = aiAnswer,
            Sources = relevantChunks.Select(c => c.DocumentName).Distinct()
        });
    }


}
