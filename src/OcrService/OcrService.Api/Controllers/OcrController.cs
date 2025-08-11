using Microsoft.AspNetCore.Mvc;
using CaptureSys.OcrService.Application.Interfaces;
using CaptureSys.Shared.Helpers;

namespace CaptureSys.OcrService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OcrController : ControllerBase
{
    private readonly ILogger<OcrController> _logger;
    private readonly IOcrProcessor _ocrProcessor;

    public OcrController(ILogger<OcrController> logger, IOcrProcessor ocrProcessor)
    {
        _logger = logger;
        _ocrProcessor = ocrProcessor;
    }

    /// <summary>
    /// Traiter un document avec OCR
    /// </summary>
    [HttpPost("process")]
    public async Task<ActionResult<OcrResult>> ProcessDocument(
        IFormFile file,
        [FromForm] string language = "eng")
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Aucun fichier fourni");
            }

            // Vérification du type de fichier - seulement les images pour l'instant
            if (!FileHelper.IsImageFile(file.FileName))
            {
                return BadRequest("Seuls les fichiers image sont supportés pour l'OCR (JPG, PNG, BMP, TIFF). Les PDF nécessitent une conversion préalable.");
            }

            _logger.LogInformation("Début du traitement OCR pour: {FileName}", file.FileName);

            var result = await _ocrProcessor.ProcessImageAsync(file.OpenReadStream(), language);

            if (result.IsFailure)
            {
                _logger.LogError("Échec du traitement OCR: {Error}", result.Error);
                return StatusCode(500, result.Error);
            }

            _logger.LogInformation("OCR terminé pour {FileName}. Texte extrait: {TextLength} caractères, Confiance: {Confidence}%", 
                file.FileName, result.Value!.ExtractedText.Length, result.Value.ConfidenceScore);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement OCR de {FileName}", file?.FileName);
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Traiter un document par chemin de fichier
    /// </summary>
    [HttpPost("process-file")]
    public async Task<ActionResult<OcrResult>> ProcessFile(
        [FromBody] ProcessFileRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.FilePath))
            {
                return BadRequest("Chemin de fichier requis");
            }

            if (!System.IO.File.Exists(request.FilePath))
            {
                return NotFound("Fichier non trouvé");
            }

            _logger.LogInformation("Début du traitement OCR pour: {FilePath}", request.FilePath);

            var result = await _ocrProcessor.ProcessDocumentAsync(request.FilePath, request.Language ?? "eng");

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement OCR de {FilePath}", request.FilePath);
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Test endpoint pour vérifier que le service fonctionne
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "OcrService", 
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5001
        });
    }
}

public class ProcessFileRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string? Language { get; set; } = "eng";
}
