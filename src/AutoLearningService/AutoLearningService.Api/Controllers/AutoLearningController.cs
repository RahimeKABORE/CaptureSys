using Microsoft.AspNetCore.Mvc;
using CaptureSys.AutoLearningService.Application.Interfaces;
using CaptureSys.AutoLearningService.Domain.Entities;


namespace CaptureSys.AutoLearningService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoLearningController : ControllerBase
{
    private readonly ILogger<AutoLearningController> _logger;
    private readonly IAutoLearningService _autoLearningService;

    public AutoLearningController(
        ILogger<AutoLearningController> logger,
        IAutoLearningService autoLearningService)
    {
        _logger = logger;
        _autoLearningService = autoLearningService;
    }

    /// <summary>
    /// Status du service
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "AutoLearningService",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5009,
            SupportedModels = Enum.GetNames<ModelType>(),
            Endpoints = new[]
            {
                "GET /api/AutoLearning - Status",
                "POST /api/AutoLearning/train - Start training",
                "GET /api/AutoLearning/job/{jobId} - Get training status",
                "GET /api/AutoLearning/jobs/active - Get active training jobs",
                "GET /api/AutoLearning/model/{jobId} - Download trained model",
                "GET /api/AutoLearning/models - List available models",
                "DELETE /api/AutoLearning/job/{jobId} - Cancel training"
            }
        });
    }

    /// <summary>
    /// Démarrer un entraînement
    /// </summary>
    [HttpPost("train")]
    public async Task<ActionResult> StartTraining(
        [FromForm] string modelName,
        [FromForm] string modelType,
        [FromForm] IFormFile dataset,
        [FromForm] string? parameters = null)
    {
        try
        {
            if (dataset == null || dataset.Length == 0)
                return BadRequest("Aucun dataset fourni");

            if (!Enum.TryParse<ModelType>(modelType, true, out var type))
                return BadRequest($"Type de modèle '{modelType}' non supporté");

            _logger.LogInformation("Démarrage de l'entraînement: {ModelName}, Type: {ModelType}", 
                modelName, modelType);

            // Sauvegarder le dataset temporairement
            var tempPath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(tempPath))
            {
                await dataset.CopyToAsync(stream);
            }

            var paramDict = new Dictionary<string, object>();
            // TODO: Parser les paramètres JSON si fournis

            var result = await _autoLearningService.StartTrainingAsync(modelName, type, tempPath, paramDict);

            if (result.IsSuccess && result.Value != null)
            {
                return Ok(new
                {
                    JobId = result.Value.Id,
                    ModelName = result.Value.ModelName,
                    Status = result.Value.Status.ToString(),
                    Message = "Entraînement démarré",
                    EstimatedTime = "~10-30 minutes"
                });
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du démarrage de l'entraînement");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtenir le statut d'un entraînement
    /// </summary>
    [HttpGet("job/{jobId:guid}")]
    public async Task<ActionResult> GetTrainingStatus(Guid jobId)
    {
        var result = await _autoLearningService.GetTrainingStatusAsync(jobId);

        if (result.IsSuccess && result.Value != null)
        {
            var job = result.Value;
            return Ok(new
            {
                JobId = job.Id,
                ModelName = job.ModelName,
                ModelType = job.ModelType.ToString(),
                Status = job.Status.ToString(),
                ProgressPercentage = job.ProgressPercentage,
                EpochsCompleted = job.EpochsCompleted,
                TotalEpochs = job.TotalEpochs,
                Metrics = job.Metrics,
                ErrorMessage = job.ErrorMessage,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt
            });
        }

        return NotFound(result.Error);
    }

    /// <summary>
    /// Lister les entraînements actifs
    /// </summary>
    [HttpGet("jobs/active")]
    public async Task<ActionResult> GetActiveTrainingJobs()
    {
        var result = await _autoLearningService.GetActiveTrainingJobsAsync();

        if (result.IsSuccess && result.Value != null)
        {
            var jobs = result.Value.Select(job => new
            {
                JobId = job.Id,
                ModelName = job.ModelName,
                ModelType = job.ModelType.ToString(),
                Status = job.Status.ToString(),
                ProgressPercentage = job.ProgressPercentage,
                StartedAt = job.StartedAt
            });

            return Ok(jobs);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Télécharger un modèle entraîné
    /// </summary>
    [HttpGet("model/{jobId:guid}")]
    public async Task<ActionResult> GetModel(Guid jobId)
    {
        var result = await _autoLearningService.GetModelAsync(jobId);

        if (result.IsSuccess && result.Value != null)
        {
            var modelPath = result.Value;
            if (System.IO.File.Exists(modelPath))
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(modelPath);
                var fileName = Path.GetFileName(modelPath);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            return NotFound("Fichier modèle introuvable");
        }

        return NotFound(result.Error);
    }

    /// <summary>
    /// Lister les modèles disponibles
    /// </summary>
    [HttpGet("models")]
    public async Task<ActionResult> GetAvailableModels()
    {
        var result = await _autoLearningService.GetAvailableModelsAsync();

        if (result.IsSuccess && result.Value != null)
        {
            return Ok(new { Models = result.Value });
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Annuler un entraînement
    /// </summary>
    [HttpDelete("job/{jobId:guid}")]
    public async Task<ActionResult> CancelTraining(Guid jobId)
    {
        var result = await _autoLearningService.CancelTrainingAsync(jobId);

        if (result.IsSuccess)
        {
            return Ok(new { Message = "Entraînement annulé avec succès" });
        }

        return BadRequest(result.Error);
    }
}
