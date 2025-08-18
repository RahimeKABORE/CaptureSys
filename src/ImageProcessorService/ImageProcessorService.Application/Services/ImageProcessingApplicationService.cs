using Microsoft.Extensions.Logging;
using CaptureSys.Shared.Results;
using CaptureSys.ImageProcessorService.Application.Interfaces;
using CaptureSys.ImageProcessorService.Domain.Entities;

namespace CaptureSys.ImageProcessorService.Application.Services;

public class ImageProcessingApplicationService : IImageProcessingService
{
    private readonly ILogger<ImageProcessingApplicationService> _logger;
    private readonly IImageProcessor _imageProcessor;
    private readonly IFileStorageService _fileStorage;
    private readonly Dictionary<Guid, ImageProcessingJob> _activeJobs;

    public ImageProcessingApplicationService(
        ILogger<ImageProcessingApplicationService> logger,
        IImageProcessor imageProcessor,
        IFileStorageService fileStorage)
    {
        _logger = logger;
        _imageProcessor = imageProcessor;
        _fileStorage = fileStorage;
        _activeJobs = new Dictionary<Guid, ImageProcessingJob>();
    }

    public async Task<Result<ImageProcessingJob>> ProcessImageAsync(
        Stream imageStream, 
        string fileName, 
        ImageOperation operation, 
        Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Démarrage du traitement d'image: {FileName}, Opération: {Operation}", 
                fileName, operation);

            var inputPath = await _fileStorage.SaveFileAsync(imageStream, fileName, "input");
            var inputSize = imageStream.Length;

            var job = new ImageProcessingJob(fileName, inputPath, operation, inputSize);
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    job.AddParameter(param.Key, param.Value);
                }
            }

            _activeJobs[job.Id] = job;
            _ = Task.Run(() => ProcessImageBackgroundAsync(job));

            return Result<ImageProcessingJob>.Success(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du démarrage du traitement d'image {FileName}", fileName);
            return Result<ImageProcessingJob>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<ImageProcessingJob>> GetJobStatusAsync(Guid jobId)
    {
        await Task.Yield();
        
        if (_activeJobs.TryGetValue(jobId, out var job))
        {
            return Result<ImageProcessingJob>.Success(job);
        }

        return Result<ImageProcessingJob>.Failure("Job non trouvé");
    }

    public async Task<Result<List<ImageProcessingJob>>> GetActiveJobsAsync()
    {
        await Task.Yield();
        var activeJobs = _activeJobs.Values.Where(j => j.Status == JobStatus.Processing).ToList();
        return Result<List<ImageProcessingJob>>.Success(activeJobs);
    }

    public async Task<Result<byte[]>> GetProcessedImageAsync(Guid jobId)
    {
        try
        {
            if (!_activeJobs.TryGetValue(jobId, out var job))
            {
                return Result<byte[]>.Failure("Job non trouvé");
            }

            if (job.Status != JobStatus.Completed || string.IsNullOrEmpty(job.OutputPath))
            {
                return Result<byte[]>.Failure("Job non terminé ou pas de résultat");
            }

            var imageData = await _fileStorage.ReadFileAsync(job.OutputPath);
            return Result<byte[]>.Success(imageData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'image traitée {JobId}", jobId);
            return Result<byte[]>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CancelJobAsync(Guid jobId)
    {
        await Task.Yield();
        
        if (_activeJobs.TryGetValue(jobId, out var job) && job.Status == JobStatus.Processing)
        {
            job.Fail("Job annulé par l'utilisateur");
            _logger.LogInformation("Job {JobId} annulé", jobId);
            return Result<bool>.Success(true);
        }

        return Result<bool>.Failure("Job non trouvé ou non annulable");
    }

    private async Task ProcessImageBackgroundAsync(ImageProcessingJob job)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Traitement de l'image {JobId} commencé", job.Id);
            job.Start();

            var inputData = await _fileStorage.ReadFileAsync(job.FilePath);
            var processedData = await _imageProcessor.ProcessAsync(inputData, job.Operation, job.Parameters);
            
            var outputPath = await _fileStorage.SaveProcessedImageAsync(
                processedData, 
                job.FileName, 
                job.Operation.ToString());

            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            job.Complete(outputPath, processedData.Length, processingTime);

            _logger.LogInformation("Traitement de l'image {JobId} terminé avec succès en {ProcessingTime}ms", 
                job.Id, processingTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de l'image {JobId}", job.Id);
            job.Fail($"Erreur de traitement: {ex.Message}");
        }
    }
}