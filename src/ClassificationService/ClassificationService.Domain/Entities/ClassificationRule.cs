using CaptureSys.Shared.Entities;

namespace CaptureSys.ClassificationService.Domain.Entities;

public class ClassificationRule : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string DocumentType { get; private set; }
    public List<string> Keywords { get; private set; }
    public List<string> Patterns { get; private set; }
    public double ConfidenceThreshold { get; private set; }
    public bool IsActive { get; private set; }
    public int Priority { get; private set; }

    public ClassificationRule(
        string name,
        string description,
        string documentType,
        List<string> keywords,
        List<string> patterns,
        double confidenceThreshold = 0.7,
        int priority = 1)
    {
        Name = name;
        Description = description;
        DocumentType = documentType;
        Keywords = keywords ?? new List<string>();
        Patterns = patterns ?? new List<string>();
        ConfidenceThreshold = confidenceThreshold;
        Priority = priority;
        IsActive = true;
    }

    public void UpdateRule(string name, string description, List<string> keywords, List<string> patterns)
    {
        Name = name;
        Description = description;
        Keywords = keywords ?? new List<string>();
        Patterns = patterns ?? new List<string>();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void SetConfidenceThreshold(double threshold)
    {
        if (threshold < 0 || threshold > 1)
            throw new ArgumentException("Confidence threshold must be between 0 and 1");
        
        ConfidenceThreshold = threshold;
    }
}