using Microsoft.AspNetCore.Mvc;
using CaptureSys.ExportService.Application.Interfaces;
using CaptureSys.ExportService.Domain.Entities;

namespace CaptureSys.ExportService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly ILogger<ExportController> _logger;
    private readonly IExportProcessor _exportProcessor;

    public ExportController(
        ILogger<ExportController> logger,
        IExportProcessor exportProcessor)
    {
        _logger = logger;
        _exportProcessor = exportProcessor;
    }

    /// <summary>
    /// Test endpoint pour vérifier que le service fonctionne
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "ExportService",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5005,
            SupportedFormats = Enum.GetNames(typeof(ExportFormat)),
            SupportedDestinations = Enum.GetNames(typeof(ExportDestination)),
            ExportStatuses = Enum.GetNames(typeof(ExportStatus))
        });
    }

    /// <summary>
    /// Créer un nouveau job d'export
    /// </summary>
    [HttpPost("jobs")]
    public async Task<ActionResult<ExportJob>> CreateExportJob([FromBody] CreateExportJobRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) || !request.DocumentIds.Any())
            {
                return BadRequest("Name et DocumentIds sont requis");
            }

            _logger.LogInformation("Création d'un job d'export {Name} pour {Count} documents", 
                request.Name, request.DocumentIds.Count);

            // Créer une configuration par défaut si non fournie
            var configuration = CreateDefaultConfiguration(request.Format, request.Destination, request.FieldsToExport);

            var result = await _exportProcessor.CreateExportJobAsync(
                request.Name, request.Format, request.Destination, 
                request.DocumentIds, configuration, request.RequestedBy ?? "System");

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            _logger.LogInformation("Job d'export créé: {JobId}", result.Value!.Id);
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du job d'export");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Traiter un job d'export
    /// </summary>
    [HttpPost("jobs/{jobId}/process")]
    public async Task<ActionResult> ProcessExportJob(Guid jobId)
    {
        try
        {
            _logger.LogInformation("Traitement du job d'export {JobId}", jobId);

            var result = await _exportProcessor.ProcessExportJobAsync(jobId);

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(new { Message = "Export traité avec succès", ResultPath = result.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement du job d'export {JobId}", jobId);
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtenir les détails d'un job d'export
    /// </summary>
    [HttpGet("jobs/{jobId}")]
    public async Task<ActionResult<ExportJob>> GetExportJob(Guid jobId)
    {
        try
        {
            var result = await _exportProcessor.GetExportJobAsync(jobId);

            if (result.IsFailure)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du job {JobId}", jobId);
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Lister tous les jobs d'export
    /// </summary>
    [HttpGet("jobs")]
    public async Task<ActionResult<List<ExportJob>>> GetExportJobs([FromQuery] string? requestedBy = null)
    {
        try
        {
            var result = await _exportProcessor.GetExportJobsAsync(requestedBy);

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des jobs d'export");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Annuler un job d'export
    /// </summary>
    [HttpPost("jobs/{jobId}/cancel")]
    public async Task<ActionResult> CancelExportJob(Guid jobId)
    {
        try
        {
            var result = await _exportProcessor.CancelExportJobAsync(jobId);

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(new { Message = "Job d'export annulé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'annulation du job {JobId}", jobId);
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtenir les configurations d'export disponibles
    /// </summary>
    [HttpGet("configurations")]
    public async Task<ActionResult<List<ExportConfiguration>>> GetExportConfigurations()
    {
        try
        {
            var result = await _exportProcessor.GetExportConfigurationsAsync();

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des configurations");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Export rapide CSV avec configuration par défaut
    /// </summary>
    [HttpPost("quick-export/csv")]
    public async Task<ActionResult> QuickExportCsv([FromBody] QuickExportRequest request)
    {
        try
        {
            var jobName = $"Quick_CSV_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            var configuration = CreateDefaultConfiguration(ExportFormat.CSV, ExportDestination.Local, request.FieldsToExport);

            var jobResult = await _exportProcessor.CreateExportJobAsync(
                jobName, ExportFormat.CSV, ExportDestination.Local,
                request.DocumentIds, configuration, request.RequestedBy ?? "System");

            if (jobResult.IsFailure)
            {
                return StatusCode(500, jobResult.Error);
            }

            var processResult = await _exportProcessor.ProcessExportJobAsync(jobResult.Value!.Id);

            if (processResult.IsFailure)
            {
                return StatusCode(500, processResult.Error);
            }

            return Ok(new 
            { 
                JobId = jobResult.Value.Id,
                ResultPath = processResult.Value,
                Message = "Export CSV terminé avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'export CSV rapide");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    private static ExportConfiguration CreateDefaultConfiguration(ExportFormat format, ExportDestination destination, List<string>? fieldsToExport)
    {
        return new ExportConfiguration(
            $"Default_{format}_{DateTime.UtcNow:yyyyMMdd}",
            format,
            destination,
            fieldsToExport ?? new List<string> { "InvoiceNumber", "TotalAmount", "DueDate", "VatAmount" }
        );
    }
}

// DTOs for requests
public record CreateExportJobRequest(
    string Name, 
    ExportFormat Format, 
    ExportDestination Destination, 
    List<Guid> DocumentIds, 
    List<string>? FieldsToExport = null,
    string? RequestedBy = null);

public record QuickExportRequest(List<Guid> DocumentIds, List<string>? FieldsToExport = null, string? RequestedBy = null);
