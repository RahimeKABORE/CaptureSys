using Microsoft.AspNetCore.Mvc;
using CaptureSys.TimerService.Application.Interfaces;
using CaptureSys.TimerService.Domain.Entities;

namespace CaptureSys.TimerService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimerController : ControllerBase
{
    private readonly ILogger<TimerController> _logger;
    private readonly ITimerService _timerService;

    public TimerController(
        ILogger<TimerController> logger,
        ITimerService timerService)
    {
        _logger = logger;
        _timerService = timerService;
    }

    /// <summary>
    /// Status du service
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "TimerService",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5011,
            SupportedTriggerTypes = Enum.GetNames<TriggerType>(),
            Endpoints = new[]
            {
                "GET /api/Timer - Status",
                "POST /api/Timer/cron - Create CRON job",
                "POST /api/Timer/simple - Create simple job",
                "POST /api/Timer/once - Create one-time job",
                "GET /api/Timer/jobs - Get all jobs",
                "GET /api/Timer/job/{jobId} - Get job details",
                "POST /api/Timer/job/{jobId}/start - Start job",
                "POST /api/Timer/job/{jobId}/pause - Pause job",
                "POST /api/Timer/job/{jobId}/stop - Stop job",
                "POST /api/Timer/job/{jobId}/trigger - Trigger job now",
                "DELETE /api/Timer/job/{jobId} - Delete job"
            }
        });
    }

    /// <summary>
    /// Créer un job CRON
    /// </summary>
    [HttpPost("cron")]
    public async Task<ActionResult> CreateCronJob([FromBody] CreateCronJobRequest request)
    {
        try
        {
            _logger.LogInformation("Création d'un job CRON: {JobName} avec l'expression {CronExpression}", 
                request.JobName, request.CronExpression);

            var result = await _timerService.CreateCronJobAsync(
                request.JobName, 
                request.JobGroup ?? "DEFAULT", 
                request.CronExpression, 
                request.TargetService, 
                request.TargetEndpoint, 
                request.JobData);

            if (result.IsSuccess && result.Value != null)
            {
                return Ok(new
                {
                    JobId = result.Value.Id,
                    JobName = result.Value.JobName,
                    Status = result.Value.Status.ToString(),
                    Message = "Job CRON créé avec succès"
                });
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du job CRON");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Créer un job simple avec intervalle
    /// </summary>
    [HttpPost("simple")]
    public async Task<ActionResult> CreateSimpleJob([FromBody] CreateSimpleJobRequest request)
    {
        try
        {
            _logger.LogInformation("Création d'un job simple: {JobName} avec l'interval {Interval}", 
                request.JobName, request.Interval);

            var result = await _timerService.CreateSimpleJobAsync(
                request.JobName, 
                request.JobGroup ?? "DEFAULT", 
                request.Interval, 
                request.TargetService, 
                request.TargetEndpoint, 
                request.MaxExecutions,
                request.JobData);

            if (result.IsSuccess && result.Value != null)
            {
                return Ok(new
                {
                    JobId = result.Value.Id,
                    JobName = result.Value.JobName,
                    Status = result.Value.Status.ToString(),
                    Message = "Job simple créé avec succès"
                });
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du job simple");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Créer un job unique
    /// </summary>
    [HttpPost("once")]
    public async Task<ActionResult> CreateOnceJob([FromBody] CreateOnceJobRequest request)
    {
        try
        {
            _logger.LogInformation("Création d'un job unique: {JobName} à exécuter le {ExecuteAt}", 
                request.JobName, request.ExecuteAt);

            var result = await _timerService.CreateOnceJobAsync(
                request.JobName, 
                request.JobGroup ?? "DEFAULT", 
                request.ExecuteAt, 
                request.TargetService, 
                request.TargetEndpoint, 
                request.JobData);

            if (result.IsSuccess && result.Value != null)
            {
                return Ok(new
                {
                    JobId = result.Value.Id,
                    JobName = result.Value.JobName,
                    Status = result.Value.Status.ToString(),
                    ExecuteAt = result.Value.NextFireTime,
                    Message = "Job unique créé avec succès"
                });
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du job unique");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Lister tous les jobs
    /// </summary>
    [HttpGet("jobs")]
    public async Task<ActionResult> GetAllJobs()
    {
        var result = await _timerService.GetAllJobsAsync();

        if (result.IsSuccess && result.Value != null)
        {
            var jobs = result.Value.Select(job => new
            {
                JobId = job.Id,
                JobName = job.JobName,
                JobGroup = job.JobGroup,
                TriggerType = job.TriggerType.ToString(),
                Status = job.Status.ToString(),
                TargetService = job.TargetService,
                TargetEndpoint = job.TargetEndpoint,
                NextFireTime = job.NextFireTime,
                LastFireTime = job.LastFireTime,
                ExecutionCount = job.ExecutionCount,
                CreatedAt = job.CreatedAt
            });

            return Ok(jobs);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Obtenir les détails d'un job
    /// </summary>
    [HttpGet("job/{jobId:guid}")]
    public async Task<ActionResult> GetJob(Guid jobId)
    {
        var result = await _timerService.GetJobAsync(jobId);

        if (result.IsSuccess && result.Value != null)
        {
            var job = result.Value;
            return Ok(new
            {
                JobId = job.Id,
                JobName = job.JobName,
                JobGroup = job.JobGroup,
                TriggerType = job.TriggerType.ToString(),
                Status = job.Status.ToString(),
                TargetService = job.TargetService,
                TargetEndpoint = job.TargetEndpoint,
                HttpMethod = job.HttpMethod,
                CronExpression = job.CronExpression,
                SimpleInterval = job.SimpleInterval,
                JobData = job.JobData,
                NextFireTime = job.NextFireTime,
                LastFireTime = job.LastFireTime,
                ExecutionCount = job.ExecutionCount,
                MaxExecutions = job.MaxExecutions,
                LastExecutionResult = job.LastExecutionResult,
                StartDate = job.StartDate,
                EndDate = job.EndDate,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt
            });
        }

        return NotFound(result.Error);
    }

    /// <summary>
    /// Démarrer un job
    /// </summary>
    [HttpPost("job/{jobId:guid}/start")]
    public async Task<ActionResult> StartJob(Guid jobId)
    {
        var result = await _timerService.StartJobAsync(jobId);

        if (result.IsSuccess)
        {
            return Ok(new { Message = "Job démarré avec succès" });
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Mettre en pause un job
    /// </summary>
    [HttpPost("job/{jobId:guid}/pause")]
    public async Task<ActionResult> PauseJob(Guid jobId)
    {
        var result = await _timerService.PauseJobAsync(jobId);

        if (result.IsSuccess)
        {
            return Ok(new { Message = "Job mis en pause avec succès" });
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Arrêter un job
    /// </summary>
    [HttpPost("job/{jobId:guid}/stop")]
    public async Task<ActionResult> StopJob(Guid jobId)
    {
        var result = await _timerService.StopJobAsync(jobId);

        if (result.IsSuccess)
        {
            return Ok(new { Message = "Job arrêté avec succès" });
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Déclencher un job immédiatement
    /// </summary>
    [HttpPost("job/{jobId:guid}/trigger")]
    public async Task<ActionResult> TriggerJob(Guid jobId)
    {
        var result = await _timerService.TriggerJobNowAsync(jobId);

        if (result.IsSuccess)
        {
            return Ok(new { Message = "Job déclenché avec succès" });
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Supprimer un job
    /// </summary>
    [HttpDelete("job/{jobId:guid}")]
    public async Task<ActionResult> DeleteJob(Guid jobId)
    {
        var result = await _timerService.DeleteJobAsync(jobId);

        if (result.IsSuccess)
        {
            return Ok(new { Message = "Job supprimé avec succès" });
        }

        return BadRequest(result.Error);
    }
}

public class CreateCronJobRequest
{
    public string JobName { get; set; } = string.Empty;
    public string? JobGroup { get; set; }
    public string CronExpression { get; set; } = string.Empty;
    public string TargetService { get; set; } = string.Empty;
    public string TargetEndpoint { get; set; } = string.Empty;
    public Dictionary<string, object>? JobData { get; set; }
}

public class CreateSimpleJobRequest
{
    public string JobName { get; set; } = string.Empty;
    public string? JobGroup { get; set; }
    public string Interval { get; set; } = string.Empty; // ex: "30s", "5m", "1h"
    public string TargetService { get; set; } = string.Empty;
    public string TargetEndpoint { get; set; } = string.Empty;
    public int? MaxExecutions { get; set; }
    public Dictionary<string, object>? JobData { get; set; }
}

public class CreateOnceJobRequest
{
    public string JobName { get; set; } = string.Empty;
    public string? JobGroup { get; set; }
    public DateTime ExecuteAt { get; set; }
    public string TargetService { get; set; } = string.Empty;
    public string TargetEndpoint { get; set; } = string.Empty;
    public Dictionary<string, object>? JobData { get; set; }
}
