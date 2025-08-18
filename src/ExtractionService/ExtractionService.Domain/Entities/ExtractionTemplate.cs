using CaptureSys.Shared.Entities;

namespace CaptureSys.ExtractionService.Domain.Entities;

public class ExtractionTemplate : BaseEntity
{
    public string Name { get; private set; }
    public string DocumentType { get; private set; }
    public string Description { get; private set; }
    public List<FieldExtractionRule> FieldRules { get; private set; }
    public bool IsActive { get; private set; }
    public int Priority { get; private set; }

    public ExtractionTemplate(
        string name,
        string documentType,
        string description,
        List<FieldExtractionRule> fieldRules,
        int priority = 1)
    {
        Name = name;
        DocumentType = documentType;
        Description = description;
        FieldRules = fieldRules ?? new List<FieldExtractionRule>();
        Priority = priority;
        IsActive = true;
    }

    public void AddFieldRule(FieldExtractionRule rule)
    {
        FieldRules.Add(rule);
    }

    public void RemoveFieldRule(string fieldName)
    {
        FieldRules.RemoveAll(r => r.FieldName == fieldName);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void UpdateTemplate(string name, string description, List<FieldExtractionRule> fieldRules)
    {
        Name = name;
        Description = description;
        FieldRules = fieldRules ?? new List<FieldExtractionRule>();
    }
}
