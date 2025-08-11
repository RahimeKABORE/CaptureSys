using System.ComponentModel.DataAnnotations;

namespace CaptureSys.Shared.DTOs;

/// <summary>
/// DTO représentant un lot de documents
/// </summary>
public record BatchDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    [Required]
    [StringLength(255)]
    public string Name { get; init; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; init; }
    
    public BatchStatus Status { get; init; } = BatchStatus.Created;
    
    [StringLength(100)]
    public string? ProjectName { get; init; }
    
    [StringLength(100)]
    public string? CreatedBy { get; init; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public DateTime? StartedAt { get; init; }
    
    public DateTime? CompletedAt { get; init; }
    
    public int TotalDocuments { get; init; }
    
    public int ProcessedDocuments { get; init; }
    
    public int SuccessfulDocuments { get; init; }
    
    public int FailedDocuments { get; init; }
    
    public int ValidationRequiredDocuments { get; init; }
    
    public double ProgressPercentage => TotalDocuments > 0 
        ? (double)ProcessedDocuments / TotalDocuments * 100 
        : 0;
    
    [StringLength(1000)]
    public string? ErrorMessage { get; init; }
    
    public Dictionary<string, object> Settings { get; init; } = new();
    
    public List<DocumentDto> Documents { get; init; } = new();
}

/// <summary>
/// Statuts possibles d'un lot
/// </summary>
public enum BatchStatus
{
    Created = 1,
    Processing = 2,
    ValidationRequired = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6,
    Paused = 7
}
