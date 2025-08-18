using Microsoft.AspNetCore.Mvc;
using CaptureSys.ClassificationService.Application.Interfaces;
using CaptureSys.ClassificationService.Domain.Entities;

namespace CaptureSys.ClassificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassificationController : ControllerBase
{
    private readonly ILogger<ClassificationController> _logger;
    private readonly IDocumentClassifier _classifier;

    public ClassificationController(
        ILogger<ClassificationController> logger,
        IDocumentClassifier classifier)
    {
        _logger = logger;
        _classifier = classifier;
    }

    /// <summary>
    /// Test endpoint pour vérifier que le service fonctionne
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "ClassificationService",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5003,
            ModelVersion = _classifier.GetModelVersion(),
            SupportedDocumentTypes = new[]
            {
                "Invoice", "Contract", "Receipt", "Identity", 
                "Medical", "Legal", "Financial", "Other"
            }
        });
    }

    /// <summary>
    /// Classifier un document par son ID et texte extrait
    /// </summary>
    [HttpPost("classify-document")]
    public async Task<ActionResult<ClassificationResult>> ClassifyDocument(
        [FromBody] ClassifyDocumentRequest request)
    {
        try
        {
            if (request == null || request.DocumentId == Guid.Empty || string.IsNullOrWhiteSpace(request.ExtractedText))
            {
                return BadRequest("DocumentId et ExtractedText sont requis");
            }

            _logger.LogInformation("Classification du document {DocumentId}", request.DocumentId);

            var result = await _classifier.ClassifyDocumentAsync(request.DocumentId, request.ExtractedText);

            if (result.IsFailure)
            {
                _logger.LogError("Échec de la classification: {Error}", result.Error);
                return StatusCode(500, result.Error);
            }

            _logger.LogInformation("Document {DocumentId} classifié comme {DocumentType} avec confiance {Confidence}%",
                request.DocumentId, result.Value!.PredictedDocumentType, result.Value.Confidence * 100);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la classification du document {DocumentId}", request?.DocumentId);
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Classifier un texte directement
    /// </summary>
    [HttpPost("classify-text")]
    public async Task<ActionResult<ClassificationResult>> ClassifyText(
        [FromBody] ClassifyTextRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Le texte est requis");
            }

            _logger.LogInformation("Classification de texte de {Length} caractères", request.Text.Length);

            var result = await _classifier.ClassifyTextAsync(request.Text);

            if (result.IsFailure)
            {
                _logger.LogError("Échec de la classification de texte: {Error}", result.Error);
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la classification de texte");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtenir toutes les classifications possibles pour un texte
    /// </summary>
    [HttpPost("get-possible-classifications")]
    public async Task<ActionResult<List<ClassificationScore>>> GetPossibleClassifications(
        [FromBody] ClassifyTextRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Le texte est requis");
            }

            var result = await _classifier.GetPossibleClassificationsAsync(request.Text);

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'obtention des classifications possibles");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Entraîner le modèle avec de nouvelles données
    /// </summary>
    [HttpPost("train-model")]
    public async Task<ActionResult> TrainModel([FromBody] TrainModelRequest request)
    {
        try
        {
            if (request?.TrainingData == null || !request.TrainingData.Any())
            {
                return BadRequest("Données d'entraînement requises");
            }

            _logger.LogInformation("Entraînement du modèle avec {Count} échantillons", request.TrainingData.Count);

            var trainingData = request.TrainingData
                .Select(td => new TrainingData(td.Text, td.DocumentType))
                .ToList();

            var result = await _classifier.TrainModelAsync(trainingData);

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(new { Message = "Modèle entraîné avec succès", ModelVersion = _classifier.GetModelVersion() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'entraînement du modèle");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Mettre à jour le modèle avec les données par défaut
    /// </summary>
    [HttpPost("update-model")]
    public async Task<ActionResult> UpdateModel()
    {
        try
        {
            _logger.LogInformation("Mise à jour du modèle avec les données par défaut");

            var result = await _classifier.UpdateModelAsync();

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(new { Message = "Modèle mis à jour avec succès", ModelVersion = _classifier.GetModelVersion() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du modèle");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }
}

// DTOs for requests
public record ClassifyDocumentRequest(Guid DocumentId, string ExtractedText);
public record ClassifyTextRequest(string Text);
public record TrainModelRequest(List<TrainingDataDto> TrainingData);
public record TrainingDataDto(string Text, string DocumentType);
