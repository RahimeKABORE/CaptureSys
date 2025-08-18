using Microsoft.AspNetCore.Mvc;
using CaptureSys.Worker.Orchestrator.Application.Interfaces;
using CaptureSys.Worker.Orchestrator.Domain.Entities;

namespace CaptureSys.Worker.Orchestrator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly ILogger<WorkflowController> _logger;
    private readonly IWorkflowOrchestrator _orchestrator;

    public WorkflowController(ILogger<WorkflowController> logger, IWorkflowOrchestrator orchestrator)
    {
        _logger = logger;
        _orchestrator = orchestrator;
    }

    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "Worker.Orchestrator",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5007,
            Endpoints = new[]
            {
                "GET /api/Workflow - Status",
                "POST /api/Workflow/start - Start workflow",
                "GET /api/Workflow/{jobId} - Get workflow status",
                "GET /api/Workflow/active - Get active workflows",
                "POST /api/Workflow/{jobId}/cancel - Cancel workflow"
            }
        });
    }

    [HttpPost("start")]
    public async Task<ActionResult<WorkflowJob>> StartWorkflow([FromBody] StartWorkflowRequest request)
    {
        try
        {
            var result = await _orchestrator.StartWorkflowAsync(request.DocumentId, request.BatchId);
            
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du démarrage du workflow");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpGet("{jobId}")]
    public async Task<ActionResult<WorkflowJob>> GetWorkflowStatus(Guid jobId)
    {
        try
        {
            var result = await _orchestrator.GetWorkflowStatusAsync(jobId);
            
            if (result.IsFailure)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du statut");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<WorkflowJob>>> GetActiveWorkflows()
    {
        try
        {
            var result = await _orchestrator.GetActiveWorkflowsAsync();
            
            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des workflows actifs");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpPost("{jobId}/cancel")]
    public async Task<ActionResult> CancelWorkflow(Guid jobId)
    {
        try
        {
            var result = await _orchestrator.CancelWorkflowAsync(jobId);
            
            if (result.IsFailure)
            {
                return NotFound(result.Error);
            }

            return Ok(new { Message = "Workflow annulé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'annulation du workflow");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }
}

public record StartWorkflowRequest(string DocumentId, string BatchId);
