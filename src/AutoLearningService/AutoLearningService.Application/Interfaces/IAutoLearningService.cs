using CaptureSys.Shared.Results;
using CaptureSys.AutoLearningService.Domain.Entities;

namespace CaptureSys.AutoLearningService.Application.Interfaces;

public interface IAutoLearningService
{
    Task<Result<TrainingJob>> StartTrainingAsync(string modelName, ModelType modelType, string datasetPath, Dictionary<string, object>? parameters = null);
    Task<Result<TrainingJob>> GetTrainingStatusAsync(Guid jobId);
    Task<Result<List<TrainingJob>>> GetActiveTrainingJobsAsync();
    Task<Result<bool>> CancelTrainingAsync(Guid jobId);
    Task<Result<string>> GetModelAsync(Guid jobId);
    Task<Result<List<string>>> GetAvailableModelsAsync();
}
