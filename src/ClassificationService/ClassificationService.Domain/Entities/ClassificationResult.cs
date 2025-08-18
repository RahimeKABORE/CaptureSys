using CaptureSys.Shared.Entities;

namespace CaptureSys.ClassificationService.Domain.Entities;

public class ClassificationResult : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public string PredictedDocumentType { get; private set; }
    public double Confidence { get; private set; }
    public List<ClassificationScore> AlternativeClassifications { get; private set; }
    public string ExtractedText { get; private set; }
    public List<string> MatchedKeywords { get; private set; }
    public List<string> MatchedPatterns { get; private set; }
    public string ModelVersion { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public bool IsValidated { get; private set; }
    public string? ValidatedBy { get; private set; }
    public string? ValidationNotes { get; private set; }

    public ClassificationResult(
        Guid documentId,
        string predictedDocumentType,
        double confidence,
        string extractedText,
        List<string> matchedKeywords,
        List<string> matchedPatterns,
        string modelVersion)
    {
        DocumentId = documentId;
        PredictedDocumentType = predictedDocumentType;
        Confidence = confidence;
        ExtractedText = extractedText;
        MatchedKeywords = matchedKeywords ?? new List<string>();
        MatchedPatterns = matchedPatterns ?? new List<string>();
        ModelVersion = modelVersion;
        ProcessedAt = DateTime.UtcNow;
        AlternativeClassifications = new List<ClassificationScore>();
        IsValidated = false;
    }

    public void AddAlternativeClassification(string documentType, double confidence)
    {
        AlternativeClassifications.Add(new ClassificationScore(documentType, confidence));
    }

    public void Validate(string validatedBy, string? notes = null)
    {
        IsValidated = true;
        ValidatedBy = validatedBy;
        ValidationNotes = notes;
    }

    public void UpdatePrediction(string documentType, double confidence)
    {
        PredictedDocumentType = documentType;
        Confidence = confidence;
        ProcessedAt = DateTime.UtcNow;
    }
}

public record ClassificationScore(string DocumentType, double Confidence);
