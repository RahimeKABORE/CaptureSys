using Microsoft.AspNetCore.Mvc;
using CaptureSys.ExtractionService.Application.Interfaces;
using CaptureSys.ExtractionService.Domain.Entities;

namespace CaptureSys.ExtractionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExtractionController : ControllerBase
{
    private readonly ILogger<ExtractionController> _logger;
    private readonly IFieldExtractor _fieldExtractor;

    public ExtractionController(
        ILogger<ExtractionController> logger,
        IFieldExtractor fieldExtractor)
    {
        _logger = logger;
        _fieldExtractor = fieldExtractor;
    }

    /// <summary>
    /// Test endpoint pour vérifier que le service fonctionne
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "ExtractionService",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5004,
            SupportedDocumentTypes = new[]
            {
                "Invoice", "Contract", "Receipt", "Identity", 
                "Medical", "Legal", "Financial"
            },
            SupportedFieldTypes = Enum.GetNames(typeof(FieldType)),
            ExtractionMethods = Enum.GetNames(typeof(ExtractionMethod))
        });
    }

    /// <summary>
    /// Extraire les champs d'un document selon son type
    /// </summary>
    [HttpPost("extract-fields")]
    public async Task<ActionResult<ExtractionResult>> ExtractFields(
        [FromBody] ExtractFieldsRequest request)
    {
        try
        {
            if (request == null || request.DocumentId == Guid.Empty || string.IsNullOrWhiteSpace(request.ExtractedText))
            {
                return BadRequest("DocumentId, DocumentType et ExtractedText sont requis");
            }

            _logger.LogInformation("Extraction de champs pour document {DocumentId} de type {DocumentType}", 
                request.DocumentId, request.DocumentType);

            var result = await _fieldExtractor.ExtractFieldsAsync(request.DocumentId, request.DocumentType, request.ExtractedText);

            if (result.IsFailure)
            {
                _logger.LogError("Échec de l'extraction: {Error}", result.Error);
                return StatusCode(500, result.Error);
            }

            _logger.LogInformation("Extraction réussie pour document {DocumentId}: {FieldCount} champs extraits",
                request.DocumentId, result.Value!.ExtractedFields.Count);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction de champs pour document {DocumentId}", request?.DocumentId);
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Extraire les champs avec un template spécifique
    /// </summary>
    [HttpPost("extract-with-template")]
    public async Task<ActionResult<ExtractionResult>> ExtractWithTemplate(
        [FromBody] ExtractWithTemplateRequest request)
    {
        try
        {
            if (request == null || request.DocumentId == Guid.Empty || string.IsNullOrWhiteSpace(request.TemplateName))
            {
                return BadRequest("DocumentId, TemplateName et ExtractedText sont requis");
            }

            var result = await _fieldExtractor.ExtractFieldsWithTemplateAsync(
                request.DocumentId, request.TemplateName, request.ExtractedText);

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction avec template");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtenir les templates disponibles pour un type de document
    /// </summary>
    [HttpGet("templates/{documentType}")]
    public async Task<ActionResult<List<ExtractionTemplate>>> GetTemplates(string documentType)
    {
        try
        {
            var result = await _fieldExtractor.GetTemplatesForDocumentTypeAsync(documentType);

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des templates pour {DocumentType}", documentType);
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }

    /// <summary>
    /// Créer un nouveau template d'extraction
    /// </summary>
    [HttpPost("templates")]
    public async Task<ActionResult<ExtractionTemplate>> CreateTemplate(
        [FromBody] CreateTemplateRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.DocumentType))
            {
                return BadRequest("Name et DocumentType sont requis");
            }

            var rules = request.FieldRules?.Select(r => new FieldExtractionRule(
                r.FieldName, r.FieldType, r.Method, r.Patterns, r.Keywords, r.IsRequired)).ToList() 
                ?? new List<FieldExtractionRule>();

            var result = await _fieldExtractor.CreateTemplateAsync(request.Name, request.DocumentType, request.Description ?? "", rules);

            if (result.IsFailure)
            {
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du template");
            return StatusCode(500, $"Erreur interne: {ex.Message}");
        }
    }
}

// DTOs for requests
public record ExtractFieldsRequest(Guid DocumentId, string DocumentType, string ExtractedText);
public record ExtractWithTemplateRequest(Guid DocumentId, string TemplateName, string ExtractedText);
public record CreateTemplateRequest(string Name, string DocumentType, string? Description, List<FieldRuleDto>? FieldRules);
public record FieldRuleDto(string FieldName, FieldType FieldType, ExtractionMethod Method, List<string> Patterns, List<string> Keywords, bool IsRequired);
