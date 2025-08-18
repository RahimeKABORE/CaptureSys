using CaptureSys.Shared.Entities;

namespace CaptureSys.ExtractionService.Domain.Entities;

public class ExtractedField : BaseEntity
{
    public string FieldName { get; private set; }
    public string? Value { get; private set; }
    public string? OriginalValue { get; private set; }
    public FieldType FieldType { get; private set; }
    public double Confidence { get; private set; }
    public ExtractionMethod ExtractionMethod { get; private set; }
    public BoundingBox? BoundingBox { get; private set; }
    public bool IsValidated { get; private set; }
    public bool IsRequired { get; private set; }
    public string? ValidationError { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; }

    public ExtractedField(
        string fieldName,
        FieldType fieldType,
        ExtractionMethod extractionMethod,
        bool isRequired = false)
    {
        FieldName = fieldName;
        FieldType = fieldType;
        ExtractionMethod = extractionMethod;
        IsRequired = isRequired;
        Confidence = 0;
        IsValidated = false;
        Metadata = new Dictionary<string, object>();
    }

    public void SetValue(string value, string originalValue, double confidence)
    {
        Value = value;
        OriginalValue = originalValue;
        Confidence = confidence;
        ValidationError = null;
    }

    public void UpdateValue(string value, double confidence)
    {
        Value = value;
        Confidence = confidence;
        ValidationError = null;
    }

    public void SetBoundingBox(BoundingBox boundingBox)
    {
        BoundingBox = boundingBox;
    }

    public void Validate()
    {
        IsValidated = true;
        ValidationError = null;
    }

    public void SetValidationError(string error)
    {
        ValidationError = error;
        IsValidated = false;
    }

    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
    }
}
