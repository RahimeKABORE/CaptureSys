using CaptureSys.Shared.DTOs;

namespace CaptureSys.Shared.Services;

/// <summary>
/// Interface du service de gestion des documents
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Créer un nouveau document
    /// </summary>
    Task<DocumentDto> CreateDocumentAsync(DocumentDto document, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtenir un document par son ID
    /// </summary>
    Task<DocumentDto?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Mettre à jour un document
    /// </summary>
    Task<DocumentDto> UpdateDocumentAsync(DocumentDto document, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Supprimer un document
    /// </summary>
    Task<bool> DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtenir les documents d'un lot
    /// </summary>
    Task<IEnumerable<DocumentDto>> GetDocumentsByBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtenir les documents par statut
    /// </summary>
    Task<IEnumerable<DocumentDto>> GetDocumentsByStatusAsync(DocumentStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Mettre à jour le statut d'un document
    /// </summary>
    Task<bool> UpdateDocumentStatusAsync(Guid documentId, DocumentStatus status, string? message = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ajouter des champs extraits à un document
    /// </summary>
    Task<bool> AddExtractedFieldsAsync(Guid documentId, IEnumerable<DocumentFieldDto> fields, CancellationToken cancellationToken = default);
}
