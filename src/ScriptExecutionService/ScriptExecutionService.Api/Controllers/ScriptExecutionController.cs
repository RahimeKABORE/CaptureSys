using Microsoft.AspNetCore.Mvc;
using CaptureSys.ScriptExecutionService.Application.Interfaces;
using CaptureSys.ScriptExecutionService.Domain.Entities;

namespace CaptureSys.ScriptExecutionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScriptExecutionController : ControllerBase
{
    private readonly ILogger<ScriptExecutionController> _logger;
    private readonly IScriptExecutionService _scriptExecutionService;

    public ScriptExecutionController(
        ILogger<ScriptExecutionController> logger,
        IScriptExecutionService scriptExecutionService)
    {
        _logger = logger;
        _scriptExecutionService = scriptExecutionService;
    }

    /// <summary>
    /// Status du service
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "ScriptExecutionService",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5010,
            SupportedScriptTypes = Enum.GetNames<ScriptType>(),
            Endpoints = new[]
            {
                "GET /api/ScriptExecution - Status",
                "POST /api/ScriptExecution/execute - Execute script",
                "GET /api/ScriptExecution/job/{jobId} - Get job status",
                "GET /api/ScriptExecution/jobs/active - Get active jobs",
                "GET /api/ScriptExecution/output/{jobId} - Get job output",
                "GET /api/ScriptExecution/script-types - Get supported script types",
                "DELETE /api/ScriptExecution/job/{jobId} - Cancel job"
            }
        });
    }

    /// <summary>
    /// Exécuter un script
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult> ExecuteScript(
        [FromForm] string scriptName,
        [FromForm] string scriptType,
        [FromForm] IFormFile scriptFile,
        [FromForm] string? parameters = null)
    {
        try
        {
            if (scriptFile == null || scriptFile.Length == 0)
                return BadRequest("Aucun fichier de script fourni");

            if (!Enum.TryParse<ScriptType>(scriptType, true, out var type))
                return BadRequest($"Type de script '{scriptType}' non supporté");

            _logger.LogInformation("Exécution de script demandée: {ScriptName}, Type: {ScriptType}", 
                scriptName, scriptType);

            // Lire le contenu du script
            string scriptContent;
            using (var reader = new StreamReader(scriptFile.OpenReadStream()))
            {
                scriptContent = await reader.ReadToEndAsync();
            }

            var paramDict = new Dictionary<string, object>();
            // TODO: Parser les paramètres JSON si fournis

            var result = await _scriptExecutionService.ExecuteScriptAsync(scriptName, type, scriptContent, paramDict);

            if (result.IsSuccess && result.Value != null)
            {
                return Ok(new
                {
                    JobId = result.Value.Id,
                    ScriptName = result.Value.ScriptName,
                    Status = result.Value.Status.ToString(),
                    Message = "Exécution démarrée",
                    EstimatedTime = "~5-30 secondes"
                });
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'exécution du script");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtenir le statut d'un job
    /// </summary>
    [HttpGet("job/{jobId:guid}")]
    public async Task<ActionResult> GetJobStatus(Guid jobId)
    {
        var result = await _scriptExecutionService.GetJobStatusAsync(jobId);

        if (result.IsSuccess && result.Value != null)
        {
            var job = result.Value;
            return Ok(new
            {
                JobId = job.Id,
                ScriptName = job.ScriptName,
                ScriptType = job.ScriptType.ToString(),
                Status = job.Status.ToString(),
                ExitCode = job.ExitCode,
                ExecutionTimeMs = job.ExecutionTimeMs,
                ErrorOutput = job.ErrorOutput,
                StartedAt = job.StartedAt,
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
        var result = await _scriptExecutionService.GetActiveJobsAsync();

        if (result.IsSuccess && result.Value != null)
        {
            var jobs = result.Value.Select(job => new
            {
                JobId = job.Id,
                ScriptName = job.ScriptName,
                ScriptType = job.ScriptType.ToString(),
                Status = job.Status.ToString(),
                StartedAt = job.StartedAt
            });

            return Ok(jobs);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Obtenir la sortie d'un job
    /// </summary>
    [HttpGet("output/{jobId:guid}")]
    public async Task<ActionResult> GetJobOutput(Guid jobId)
    {
        var result = await _scriptExecutionService.GetJobOutputAsync(jobId);

        if (result.IsSuccess && result.Value != null)
        {
            return Ok(new { Output = result.Value });
        }

        return NotFound(result.Error);
    }

    /// <summary>
    /// Lister les types de scripts supportés
    /// </summary>
    [HttpGet("script-types")]
    public async Task<ActionResult> GetSupportedScriptTypes()
    {
        var result = await _scriptExecutionService.GetSupportedScriptTypesAsync();

        if (result.IsSuccess && result.Value != null)
        {
            return Ok(new { ScriptTypes = result.Value.Select(t => t.ToString()) });
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Annuler un job
    /// </summary>
    [HttpDelete("job/{jobId:guid}")]
    public async Task<ActionResult> CancelJob(Guid jobId)
    {
        var result = await _scriptExecutionService.CancelJobAsync(jobId);

        if (result.IsSuccess)
        {
            return Ok(new { Message = "Job annulé avec succès" });
        }

        return BadRequest(result.Error);
    }
}
