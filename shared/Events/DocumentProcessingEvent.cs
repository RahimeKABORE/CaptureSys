using CaptureSys.Shared.DTOs;
using MediatR;

namespace CaptureSys.Shared.Events;

/// <summary>
/// Événement de base pour le traitement des documents
/// </summary>
public abstract record DocumentProcessingEvent : INotification
{
    public Guid DocumentId { get; init; }
    public Guid BatchId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Document ingéré et prêt pour traitement
/// </summary>
public record DocumentIngestedEvent : DocumentProcessingEvent
{
    public string FileName { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string? MimeType { get; init; }
    public long FileSize { get; init; }
}

/// <summary>
/// Traitement d'image terminé
/// </summary>
public record DocumentImageProcessedEvent : DocumentProcessingEvent
{
    public string ProcessedImagePath { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// OCR terminé
/// </summary>
public record DocumentOcrCompletedEvent : DocumentProcessingEvent
{
    public string OcrText { get; init; } = string.Empty;
    public double ConfidenceScore { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Classification terminée
/// </summary>
public record DocumentClassifiedEvent : DocumentProcessingEvent
{
    public string DocumentType { get; init; } = string.Empty;
    public double ConfidenceScore { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Extraction terminée
/// </summary>
public record DocumentFieldsExtractedEvent : DocumentProcessingEvent
{
    public List<DocumentFieldDto> ExtractedFields { get; init; } = new();
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Validation requise
/// </summary>
public record DocumentValidationRequiredEvent : DocumentProcessingEvent
{
    public List<string> ValidationErrors { get; init; } = new();
    public List<DocumentFieldDto> FieldsToValidate { get; init; } = new();
}

/// <summary>
/// Traitement terminé
/// </summary>
public record DocumentProcessingCompletedEvent : DocumentProcessingEvent
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan ProcessingDuration { get; init; }
}
