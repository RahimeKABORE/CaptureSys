using Microsoft.AspNetCore.Mvc;

namespace CaptureSys.IngestionService.Api.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Endpoint de santé de l'API
    /// </summary>
    [HttpGet]
    public ActionResult GetHealth()
    {
        return Ok(new
        {
            Service = "CaptureSys Ingestion Service",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }

    /// <summary>
    /// Endpoint de santé détaillé
    /// </summary>
    [HttpGet("health")]
    public ActionResult GetDetailedHealth()
    {
        return Ok(new
        {
            Service = "CaptureSys Ingestion Service",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Endpoints = new[]
            {
                "GET / - Health check",
                "GET /health - Detailed health",
                "GET /swagger - API Documentation",
                "POST /api/documents/upload - Upload document",
                "GET /api/documents/{id} - Get document",
                "GET /api/documents/batch/{batchId} - Get batch documents"
            }
        });
    }
}
