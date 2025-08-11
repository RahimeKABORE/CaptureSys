using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaptureSys.Shared.DTOs;
using CaptureSys.Shared.Helpers;
using CaptureSys.Shared.Events;
using CaptureSys.IngestionService.Infrastructure.Data;
using CaptureSys.IngestionService.Application.Interfaces;
using CaptureSys.IngestionService.Domain.Entities;

namespace CaptureSys.IngestionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ILogger<DocumentsController> _logger;
    private readonly IngestionDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IEventPublisher _eventPublisher;

    public DocumentsController(
        ILogger<DocumentsController> logger,
        IngestionDbContext context,
        IFileStorageService fileStorage,
        IEventPublisher eventPublisher)
    {
        _logger = logger;
        _context = context;
        _fileStorage = fileStorage;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Test endpoint pour vérifier que le service fonctionne
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "IngestionService",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5000,
            Endpoints = new[]
            {
                "GET /api/Documents - Status",
                "POST /api/Documents/upload - Upload document",
                "GET /api/Documents/{id} - Get document",
                "GET /api/Documents/batch/{batchId} - Get batch documents"
            }
        });
    }

    /// <summary>
    /// Upload un document pour traitement
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<DocumentDto>> UploadDocument(
        IFormFile file, 
        [FromForm] string? batchName = null)
    {
        try
        {
            _logger.LogInformation("Début upload - File: {FileNull}, BatchName: {BatchName}", 
                file == null ? "NULL" : $"{file.FileName} ({file.Length} bytes)", batchName ?? "NULL");

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Fichier manquant ou vide");
                return BadRequest("Aucun fichier fourni");
            }

            // Validation du type de fichier
            if (!FileHelper.IsImageFile(file.FileName) && !FileHelper.IsDocumentFile(file.FileName))
            {
                _logger.LogWarning("Type de fichier non supporté: {FileName}", file.FileName);
                return BadRequest("Type de fichier non supporté");
            }

            _logger.LogInformation("Réception du fichier: {FileName} ({FileSize} bytes)", 
                file.FileName, file.Length);

            // Créer ou récupérer le batch
            _logger.LogDebug("Création/récupération du batch: {BatchName}", batchName ?? "Default");
            var batch = await GetOrCreateBatchAsync(batchName ?? "Default");

            // Créer l'entité document
            _logger.LogDebug("Création de l'entité document");
            var document = new Document(
                file.FileName,
                batch.Id,
                FileHelper.GetMimeType(file.FileName),
                file.Length);

            // Sauvegarder le fichier
            _logger.LogDebug("Sauvegarde du fichier physique");
            var filePath = await _fileStorage.SaveFileAsync(
                file.OpenReadStream(), 
                file.FileName, 
                batch.Id.ToString());

            document.UpdateFilePath(filePath);

            // Sauvegarder en base
            _logger.LogDebug("Sauvegarde en base de données");
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Publier l'événement d'ingestion
            _logger.LogDebug("Publication de l'événement");
            var ingestedEvent = new DocumentIngestedEvent
            {
                DocumentId = document.Id,
                BatchId = batch.Id,
                FileName = document.FileName,
                FilePath = filePath,
                MimeType = document.MimeType,
                FileSize = document.FileSize ?? 0,
                CorrelationId = Guid.NewGuid().ToString()
            };

            await _eventPublisher.PublishAsync(ingestedEvent);

            _logger.LogInformation("Document {DocumentId} ingéré avec succès", document.Id);

            // Mapper vers DTO
            var documentDto = MapToDto(document);
            return Ok(documentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ingestion du document {FileName}. Détails: {ErrorMessage}", 
                file?.FileName, ex.Message);
            return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtenir le statut d'un document
    /// </summary>
    [HttpGet("{documentId:guid}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(Guid documentId)
    {
        try
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                return NotFound($"Document {documentId} non trouvé");
            }

            var documentDto = MapToDto(document);
            return Ok(documentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du document {DocumentId}", documentId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Lister les documents d'un batch
    /// </summary>
    [HttpGet("batch/{batchId:guid}")]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocumentsByBatch(Guid batchId)
    {
        try
        {
            var documents = await _context.Documents
                .Where(d => d.BatchId == batchId)
                .ToListAsync();

            var documentDtos = documents.Select(MapToDto);
            return Ok(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des documents du batch {BatchId}", batchId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    private async Task<Batch> GetOrCreateBatchAsync(string batchName)
    {
        var batch = await _context.Batches
            .FirstOrDefaultAsync(b => b.Name == batchName && b.Status != BatchStatus.Completed);

        if (batch == null)
        {
            batch = new Batch(batchName, $"Batch créé automatiquement le {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            _context.Batches.Add(batch);
            await _context.SaveChangesAsync();
        }

        return batch;
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            FileName = document.FileName,
            DocumentType = document.DocumentType,
            Status = document.Status,
            BatchId = document.BatchId,
            FilePath = document.FilePath,
            FileSize = document.FileSize,
            MimeType = document.MimeType,
            PageCount = document.PageCount,
            CreatedAt = document.CreatedAt,
            ProcessedAt = document.ProcessedAt,
            ProcessedBy = document.ProcessedBy,
            ConfidenceScore = document.ConfidenceScore,
            ErrorMessage = document.ErrorMessage,
            Metadata = document.Metadata
        };
    }
}