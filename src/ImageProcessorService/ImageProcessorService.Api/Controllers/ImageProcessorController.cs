using Microsoft.AspNetCore.Mvc;
using CaptureSys.ImageProcessorService.Application.Interfaces;
using CaptureSys.ImageProcessorService.Domain.Entities;

namespace CaptureSys.ImageProcessorService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageProcessorController : ControllerBase
{
    private readonly ILogger<ImageProcessorController> _logger;
    private readonly IImageProcessingService _imageProcessingService;

    public ImageProcessorController(
        ILogger<ImageProcessorController> logger,
        IImageProcessingService imageProcessingService)
    {
        _logger = logger;
        _imageProcessingService = imageProcessingService;
    }

    /// <summary>
    /// Status du service
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "ImageProcessorService",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5008,
            SupportedOperations = Enum.GetNames<ImageOperation>(),
            Endpoints = new[]
            {
                "GET /api/ImageProcessor - Status",
                "POST /api/ImageProcessor/process - Process image",
                "GET /api/ImageProcessor/job/{jobId} - Get job status",
                "GET /api/ImageProcessor/jobs/active - Get active jobs",
                "GET /api/ImageProcessor/result/{jobId} - Get processed image",
                "DELETE /api/ImageProcessor/job/{jobId} - Cancel job"
            }
        });
    }

    /// <summary>
    /// Traiter une image
    /// </summary>
    [HttpPost("process")]
    public async Task<ActionResult> ProcessImage(
        [FromForm] IFormFile file,
        [FromForm] string operation,
        [FromForm] string? parameters = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier fourni");

            if (!Enum.TryParse<ImageOperation>(operation, true, out var op))
                return BadRequest($"Opération '{operation}' non supportée");

            _logger.LogInformation("Traitement d'image demandé: {FileName}, Opération: {Operation}",
                file.FileName, operation);

            var paramDict = new Dictionary<string, object>();

            var result = await _imageProcessingService.ProcessImageAsync(
                file.OpenReadStream(),
                file.FileName ?? "unknown",
                op,
                paramDict);

            if (result.IsSuccess && result.Value != null)
            {
                return Ok(new
                {
                    JobId = result.Value.Id,
                    Status = result.Value.Status.ToString(),
                    Message = "Traitement démarré",
                    EstimatedTime = "~2-5 secondes"
                });
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement d'image");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtenir le statut d'un job
    /// </summary>
    [HttpGet("job/{jobId:guid}")]
    public async Task<ActionResult> GetJobStatus(Guid jobId)
    {
        var result = await _imageProcessingService.GetJobStatusAsync(jobId);

        if (result.IsSuccess && result.Value != null)
        {
            var job = result.Value;
            return Ok(new
            {
                JobId = job.Id,
                FileName = job.FileName,
                Operation = job.Operation.ToString(),
                Status = job.Status.ToString(),
                ProcessingTimeMs = job.ProcessingTimeMs,
                InputSize = job.InputSize,
                OutputSize = job.OutputSize,
                ErrorMessage = job.ErrorMessage,
                CreatedAt = job.CreatedAt,
                CompletedAt = job.CompletedAt
            });
        }

        return NotFound(result.Error);
    }

    /// <summary>
    /// Lister les jobs actifs
    /// </summary>
    [HttpGet("jobs/active")]
    public async Task<ActionResult> GetActiveJobs()
    {
        var result = await _imageProcessingService.GetActiveJobsAsync();

        if (result.IsSuccess && result.Value != null)
        {
            var jobs = result.Value.Select(job => new
            {
                JobId = job.Id,
                FileName = job.FileName,
                Operation = job.Operation.ToString(),
                Status = job.Status.ToString(),
                CreatedAt = job.CreatedAt
            });

            return Ok(jobs);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Télécharger l'image traitée
    /// </summary>
    [HttpGet("result/{jobId:guid}")]
    public async Task<ActionResult> GetProcessedImage(Guid jobId)
    {
        var result = await _imageProcessingService.GetProcessedImageAsync(jobId);

        if (result.IsSuccess && result.Value != null)
        {
            var jobResult = await _imageProcessingService.GetJobStatusAsync(jobId);
            var contentType = "application/octet-stream";
            var fileName = (jobResult.IsSuccess && jobResult.Value != null) ? jobResult.Value.FileName : "processed_image";

            return File(result.Value, contentType, fileName);
        }

        return NotFound(result.Error);
    }

    /// <summary>
    /// Annuler un job
    /// </summary>
    [HttpDelete("job/{jobId:guid}")]
    public async Task<ActionResult> CancelJob(Guid jobId)
    {
        var result = await _imageProcessingService.CancelJobAsync(jobId);

        if (result.IsSuccess)
        {
            return Ok(new { Message = "Job annulé avec succès" });
        }

        return BadRequest(result.Error);
    }
}