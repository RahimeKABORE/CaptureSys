using CaptureSys.Shared.Entities;
using CaptureSys.Shared.DTOs;

namespace CaptureSys.IngestionService.Domain.Entities;

public class Document : BaseEntity
{
    public string FileName { get; private set; } = string.Empty;
    public string? DocumentType { get; private set; }
    public DocumentStatus Status { get; private set; }
    public Guid BatchId { get; private set; }
    public string? FilePath { get; private set; }
    public long? FileSize { get; private set; }
    public string? MimeType { get; private set; }
    public int PageCount { get; private set; } = 1;
    public DateTime? ProcessedAt { get; private set; }
    public string? ProcessedBy { get; private set; }
    public double? ConfidenceScore { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private Document() { } // EF Constructor

    public Document(string fileName, Guid batchId, string? mimeType, long? fileSize)
    {
        FileName = fileName;
        BatchId = batchId;
        MimeType = mimeType;
        FileSize = fileSize;
        Status = DocumentStatus.Ingested;
    }

    public void UpdateFilePath(string filePath)
    {
        FilePath = filePath;
        MarkAsUpdated();
    }

    public void UpdateStatus(DocumentStatus status, string? message = null)
    {
        Status = status;
        if (!string.IsNullOrEmpty(message))
        {
            ErrorMessage = message;
        }
        MarkAsUpdated();
    }
}
