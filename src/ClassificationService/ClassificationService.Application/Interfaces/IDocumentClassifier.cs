using CaptureSys.ClassificationService.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.ClassificationService.Application.Interfaces;

public interface IDocumentClassifier
{
    Task<Result<ClassificationResult>> ClassifyDocumentAsync(Guid documentId, string extractedText);
    Task<Result<ClassificationResult>> ClassifyTextAsync(string text);
    Task<Result<List<ClassificationScore>>> GetPossibleClassificationsAsync(string text);
    Task<Result<bool>> TrainModelAsync(List<TrainingData> trainingData);
    Task<Result<bool>> UpdateModelAsync();
    string GetModelVersion();
}

public record TrainingData(string Text, string DocumentType);
