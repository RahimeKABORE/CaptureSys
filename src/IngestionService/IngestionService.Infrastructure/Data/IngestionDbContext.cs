using Microsoft.EntityFrameworkCore;
using CaptureSys.IngestionService.Domain.Entities;
using System.Text.Json;

namespace CaptureSys.IngestionService.Infrastructure.Data;

public class IngestionDbContext : DbContext
{
    public IngestionDbContext(DbContextOptions<IngestionDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<Batch> Batches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration Document
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.DocumentType).HasMaxLength(100);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.MimeType).HasMaxLength(50);
            entity.Property(e => e.ProcessedBy).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            
            // Conversion JSON pour Metadata
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new());
        });

        // Configuration Batch
        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ProjectName).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            
            // Conversion JSON pour Settings
            entity.Property(e => e.Settings)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new());

            // Relation avec Documents
            entity.HasMany(b => b.Documents)
                .WithOne()
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
