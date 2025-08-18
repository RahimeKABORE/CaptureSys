using CaptureSys.Shared.Entities;

namespace CaptureSys.ExtractionService.Domain.Entities;

public class FieldExtractionRule : BaseEntity
{
    public string FieldName { get; private set; }
    public FieldType FieldType { get; private set; }
    public List<string> Patterns { get; private set; }
    public List<string> Keywords { get; private set; }
    public ExtractionMethod Method { get; private set; }
    public bool IsRequired { get; private set; }
    public string? ValidationRule { get; private set; }
    public string? DefaultValue { get; private set; }
    public BoundingBox? SearchArea { get; private set; }

    public FieldExtractionRule(
        string fieldName,
        FieldType fieldType,
        ExtractionMethod method,
        List<string> patterns,
        List<string> keywords,
        bool isRequired = false)
    {
        FieldName = fieldName;
        FieldType = fieldType;
        Method = method;
        Patterns = patterns ?? new List<string>();
        Keywords = keywords ?? new List<string>();
        IsRequired = isRequired;
    }

    public void SetSearchArea(BoundingBox area)
    {
        SearchArea = area;
    }

    public void SetValidationRule(string rule)
    {
        ValidationRule = rule;
    }

    public void SetDefaultValue(string value)
    {
        DefaultValue = value;
    }

    public void UpdateRule(List<string> patterns, List<string> keywords)
    {
        Patterns = patterns ?? new List<string>();
        Keywords = keywords ?? new List<string>();
    }
}

public enum FieldType
{
    Text = 1,
    Number = 2,
    Date = 3,
    Amount = 4,
    Email = 5,
    Phone = 6,
    Address = 7,
    Boolean = 8,
    Custom = 9
}

public enum ExtractionMethod
{
    Regex = 1,
    Keyword = 2,
    Position = 3,
    Template = 4,
    ML = 5,
    OCR = 6
}

public record BoundingBox(int X, int Y, int Width, int Height, int PageNumber);
