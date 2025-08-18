using CaptureSys.Shared.Entities;

namespace CaptureSys.ExtractionService.Domain.Entities;

public class ExtractionResult : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public string DocumentType { get; private set; }
    public List<ExtractedField> ExtractedFields { get; private set; }
    public string TemplateUsed { get; private set; }
    public double OverallConfidence { get; private set; }
    public TimeSpan ProcessingTime { get; private set; }
    public string ProcessedBy { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; }
    public bool IsValidated { get; private set; }
    public string? ValidatedBy { get; private set; }
    public DateTime? ValidatedAt { get; private set; }

    public ExtractionResult(
        Guid documentId,
        string documentType,
        string templateUsed,
        List<ExtractedField> extractedFields,
        TimeSpan processingTime,
        string processedBy)
    {
        DocumentId = documentId;
        DocumentType = documentType;
        TemplateUsed = templateUsed;
        ExtractedFields = extractedFields ?? new List<ExtractedField>();
        ProcessingTime = processingTime;
        ProcessedBy = processedBy;
        Metadata = new Dictionary<string, object>();
        IsValidated = false;
        
        CalculateOverallConfidence();
    }

    public void AddExtractedField(ExtractedField field)
    {
        ExtractedFields.Add(field);
        CalculateOverallConfidence();
    }

    public void UpdateField(string fieldName, string value, double confidence)
    {
        var field = ExtractedFields.FirstOrDefault(f => f.FieldName == fieldName);
        if (field != null)
        {
            field.UpdateValue(value, confidence);
            CalculateOverallConfidence();
        }
    }

    public void Validate(string validatedBy)
    {
        IsValidated = true;
        ValidatedBy = validatedBy;
        ValidatedAt = DateTime.UtcNow;
    }

    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
    }

    private void CalculateOverallConfidence()
    {
        if (!ExtractedFields.Any())
        {
            OverallConfidence = 0;
            return;
        }

        OverallConfidence = ExtractedFields.Average(f => f.Confidence);
    }
}
