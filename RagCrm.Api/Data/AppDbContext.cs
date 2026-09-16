using Microsoft.EntityFrameworkCore;
using RagCrm.Api.Models;

namespace RagCrm.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<User> Users => Set<User>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasPostgresExtension("vector");

    modelBuilder.Entity<DocumentChunk>(entity =>
    {
        // Explicitly define 768 dimensions for the vector column
        entity.Property(c => c.Embedding)
              .HasColumnType("vector(768)");

        entity.HasIndex(c => c.Embedding)
              .HasMethod("hnsw")
              .HasOperators("vector_cosine_ops");
    });

    base.OnModelCreating(modelBuilder);
}

}
