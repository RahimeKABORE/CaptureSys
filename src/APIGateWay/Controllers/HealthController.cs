using Microsoft.AspNetCore.Mvc;

namespace CaptureSys.ApiGateway.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check de l'API Gateway
    /// </summary>
    [HttpGet]
    public ActionResult GetHealth()
    {
        return Ok(new
        {
            Service = "CaptureSys API Gateway",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Routes = new
            {
                Ingestion = "http://localhost:5002/ingestion/",
                OCR = "http://localhost:5002/ocr/",
                Swagger = "http://localhost:5002/swagger"
            }
        });
    }

    /// <summary>
    /// Statut des services downstream
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult> GetServicesStatus()
    {
        var services = new List<object>();

        // Test IngestionService
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            var response = await httpClient.GetAsync("http://localhost:5000/api/Documents");
            services.Add(new
            {
                Service = "IngestionService",
                Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                Url = "http://localhost:5000",
                ResponseTime = "< 5s"
            });
        }
        catch (Exception ex)
        {
            services.Add(new
            {
                Service = "IngestionService",
                Status = "Unavailable",
                Url = "http://localhost:5000",
                Error = ex.Message
            });
        }

        // Test OcrService
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            var response = await httpClient.GetAsync("http://localhost:5001/api/Ocr");
            services.Add(new
            {
                Service = "OcrService",
                Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                Url = "http://localhost:5001",
                ResponseTime = "< 5s"
            });
        }
        catch (Exception ex)
        {
            services.Add(new
            {
                Service = "OcrService",
                Status = "Unavailable",
                Url = "http://localhost:5001",
                Error = ex.Message
            });
        }

        return Ok(new
        {
            Gateway = "CaptureSys API Gateway",
            Timestamp = DateTime.UtcNow,
            Services = services
        });
    }
}
