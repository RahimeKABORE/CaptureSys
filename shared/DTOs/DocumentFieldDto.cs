using System.ComponentModel.DataAnnotations;

namespace CaptureSys.Shared.DTOs;

/// <summary>
/// DTO représentant un champ extrait d'un document
/// </summary>
public record DocumentFieldDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public Guid DocumentId { get; init; }
    
    [Required]
    [StringLength(100)]
    public string FieldName { get; init; } = string.Empty;
    
    [StringLength(2000)]
    public string? Value { get; init; }
    
    [StringLength(2000)]
    public string? OriginalValue { get; init; }
    
    public double ConfidenceScore { get; init; }
    
    public FieldType FieldType { get; init; } = FieldType.Text;
    
    public bool IsValidated { get; init; }
    
    public bool IsRequired { get; init; }
    
    [StringLength(500)]
    public string? ValidationRule { get; init; }
    
    [StringLength(1000)]
    public string? ValidationError { get; init; }
    
    public ExtractionMethod ExtractionMethod { get; init; } = ExtractionMethod.Ocr;
    
    public BoundingBoxDto? BoundingBox { get; init; }
    
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Types de champs supportés
/// </summary>
public enum FieldType
{
    Text = 1,
    Number = 2,
    Date = 3,
    Currency = 4,
    Email = 5,
    Phone = 6,
    Checkbox = 7,
    Barcode = 8,
    Signature = 9
}

/// <summary>
/// Méthodes d'extraction
/// </summary>
public enum ExtractionMethod
{
    Ocr = 1,
    Template = 2,
    MachineLearning = 3,
    Barcode = 4,
    Manual = 5,
    Script = 6
}

/// <summary>
/// Zone de délimitation d'un champ
/// </summary>
public record BoundingBoxDto
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int PageNumber { get; init; } = 1;
}
