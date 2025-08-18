using CaptureSys.Shared.Entities;

namespace CaptureSys.ExportService.Domain.Entities;

public class ExportedDocument : BaseEntity
{
    public Guid OriginalDocumentId { get; private set; }
    public Guid ExportJobId { get; private set; }
    public string FileName { get; private set; }
    public string DocumentType { get; private set; }
    public Dictionary<string, object> ExportedFields { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; }
    public ExportStatus ExportStatus { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime ExportedAt { get; private set; }

    public ExportedDocument(
        Guid originalDocumentId,
        Guid exportJobId,
        string fileName,
        string documentType,
        Dictionary<string, object> exportedFields,
        Dictionary<string, object>? metadata = null)
    {
        OriginalDocumentId = originalDocumentId;
        ExportJobId = exportJobId;
        FileName = fileName;
        DocumentType = documentType;
        ExportedFields = exportedFields ?? new Dictionary<string, object>();
        Metadata = metadata ?? new Dictionary<string, object>();
        ExportStatus = ExportStatus.Pending;
        ExportedAt = DateTime.UtcNow;
    }

    public void MarkAsExported()
    {
        ExportStatus = ExportStatus.Completed;
        ExportedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        ExportStatus = ExportStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void AddField(string fieldName, object value)
    {
        ExportedFields[fieldName] = value;
    }

    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
    }

    public T? GetField<T>(string fieldName)
    {
        return ExportedFields.TryGetValue(fieldName, out var value) && value is T typedValue ? typedValue : default;
    }
}
