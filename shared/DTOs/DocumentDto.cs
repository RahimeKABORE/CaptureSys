using System.ComponentModel.DataAnnotations;

namespace CaptureSys.Shared.DTOs;

/// <summary>
/// DTO représentant un document dans le système
/// </summary>
public record DocumentDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    [Required]
    [StringLength(255)]
    public string FileName { get; init; } = string.Empty;
    
    [StringLength(100)]
    public string? DocumentType { get; init; }
    
    [Required]
    public DocumentStatus Status { get; init; } = DocumentStatus.Uploaded;
    
    public Guid BatchId { get; init; }
    
    [StringLength(500)]
    public string? FilePath { get; init; }
    
    public long? FileSize { get; init; }
    
    [StringLength(50)]
    public string? MimeType { get; init; }
    
    public int PageCount { get; init; } = 1;
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public DateTime? ProcessedAt { get; init; }
    
    [StringLength(100)]
    public string? ProcessedBy { get; init; }
    
    public double? ConfidenceScore { get; init; }
    
    [StringLength(1000)]
    public string? ErrorMessage { get; init; }
    
    public Dictionary<string, object> Metadata { get; init; } = new();
    
    public List<DocumentFieldDto> ExtractedFields { get; init; } = new();
}

/// <summary>
/// Statuts possibles d'un document
/// </summary>
public enum DocumentStatus
{
    Uploaded = 1,
    Ingested = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6
}
