using Microsoft.Extensions.Logging;
using CaptureSys.Shared.Results;
using CaptureSys.AutoLearningService.Application.Interfaces;
using CaptureSys.AutoLearningService.Domain.Entities;

namespace CaptureSys.AutoLearningService.Application.Services;

public class AutoLearningApplicationService : IAutoLearningService
{
    private readonly ILogger<AutoLearningApplicationService> _logger;
    private readonly Dictionary<Guid, TrainingJob> _activeJobs;

    public AutoLearningApplicationService(ILogger<AutoLearningApplicationService> logger)
    {
        _logger = logger;
        _activeJobs = new Dictionary<Guid, TrainingJob>();
    }

    public Task<Result<TrainingJob>> StartTrainingAsync(string modelName, ModelType modelType, string datasetPath, Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Démarrage de l'entraînement: {ModelName}, Type: {ModelType}", modelName, modelType);

            var job = new TrainingJob(modelName, modelType, datasetPath);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    job.AddParameter(param.Key, param.Value);
                }
            }

            _activeJobs[job.Id] = job;
            
            // Démarrer l'entraînement en arrière-plan sans attendre
            _ = Task.Run(async () => await SimulateTrainingAsync(job));

            return Task.FromResult(Result<TrainingJob>.Success(job));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du démarrage de l'entraînement");
            return Task.FromResult(Result<TrainingJob>.Failure($"Erreur: {ex.Message}"));
        }
    }

    private async Task SimulateTrainingAsync(TrainingJob job)
    {
        try
        {
            _logger.LogInformation("Simulation de l'entraînement {JobId} commencée", job.Id);
            job.Start();

            for (int epoch = 1; epoch <= 10; epoch++)
            {
                await Task.Delay(2000);
                var progress = (epoch / 10.0) * 100;
                job.UpdateProgress(progress, epoch);
                _logger.LogInformation("Entraînement {JobId} - Epoch {Epoch}/10 ({Progress}%)", job.Id, epoch, progress);
            }

            var metrics = new TrainingMetrics
            {
                Accuracy = 0.95,
                Precision = 0.93,
                Recall = 0.94,
                F1Score = 0.935,
                Loss = 0.15,
                SamplesProcessed = 1000
            };

            var outputPath = Path.Combine(Path.GetTempPath(), $"{job.ModelName}_{job.Id}.model");
            await File.WriteAllTextAsync(outputPath, "Modèle simulé");

            job.Complete(outputPath, metrics);
            _logger.LogInformation("Entraînement {JobId} terminé avec succès", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'entraînement {JobId}", job.Id);
            job.Fail($"Erreur d'entraînement: {ex.Message}");
        }
    }

    public Task<Result<TrainingJob>> GetTrainingStatusAsync(Guid jobId)
    {
        if (_activeJobs.TryGetValue(jobId, out var job))
        {
            return Task.FromResult(Result<TrainingJob>.Success(job));
        }

        return Task.FromResult(Result<TrainingJob>.Failure("Job d'entraînement non trouvé"));
    }

    public Task<Result<List<TrainingJob>>> GetActiveTrainingJobsAsync()
    {
        var activeJobs = _activeJobs.Values.Where(j => j.Status == TrainingStatus.Training).ToList();
        return Task.FromResult(Result<List<TrainingJob>>.Success(activeJobs));
    }

    public Task<Result<bool>> CancelTrainingAsync(Guid jobId)
    {
        if (_activeJobs.TryGetValue(jobId, out var job) && job.Status == TrainingStatus.Training)
        {
            job.Fail("Entraînement annulé par l'utilisateur");
            _logger.LogInformation("Entraînement {JobId} annulé", jobId);
            return Task.FromResult(Result<bool>.Success(true));
        }

        return Task.FromResult(Result<bool>.Failure("Job non trouvé ou non annulable"));
    }

    public Task<Result<string>> GetModelAsync(Guid jobId)
    {
        if (_activeJobs.TryGetValue(jobId, out var job) && job.Status == TrainingStatus.Completed && !string.IsNullOrEmpty(job.OutputModelPath))
        {
            return Task.FromResult(Result<string>.Success(job.OutputModelPath));
        }

        return Task.FromResult(Result<string>.Failure("Modèle non trouvé ou entraînement non terminé"));
    }

    public Task<Result<List<string>>> GetAvailableModelsAsync()
    {
        var completedModels = _activeJobs.Values
            .Where(j => j.Status == TrainingStatus.Completed && !string.IsNullOrEmpty(j.OutputModelPath))
            .Select(j => j.ModelName)
            .ToList();
        
        return Task.FromResult(Result<List<string>>.Success(completedModels));
    }
}