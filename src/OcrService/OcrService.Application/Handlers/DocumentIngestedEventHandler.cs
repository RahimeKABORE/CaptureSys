using MediatR;
using Microsoft.Extensions.Logging;
using CaptureSys.Shared.Events;
using CaptureSys.OcrService.Application.Interfaces;

namespace CaptureSys.OcrService.Application.Handlers;

public class DocumentIngestedEventHandler : INotificationHandler<DocumentIngestedEvent>
{
    private readonly ILogger<DocumentIngestedEventHandler> _logger;
    private readonly IOcrProcessor _ocrProcessor;

    public DocumentIngestedEventHandler(
        ILogger<DocumentIngestedEventHandler> logger,
        IOcrProcessor ocrProcessor)
    {
        _logger = logger;
        _ocrProcessor = ocrProcessor;
    }

    public async Task Handle(DocumentIngestedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Traitement OCR du document ingéré: {DocumentId} - {FileName}", 
                notification.DocumentId, notification.FileName);

            // Vérifier si le fichier est traitable par OCR
            if (!IsOcrProcessable(notification.MimeType))
            {
                _logger.LogInformation("Fichier {FileName} non traitable par OCR (type: {MimeType})", 
                    notification.FileName, notification.MimeType);
                return;
            }

            // Traitement OCR
            var result = await _ocrProcessor.ProcessDocumentAsync(notification.FilePath);

            if (result.IsSuccess)
            {
                _logger.LogInformation("OCR réussi pour {DocumentId}. Texte extrait: {TextLength} caractères, Confiance: {Confidence}%",
                    notification.DocumentId, result.Value!.ExtractedText.Length, result.Value.ConfidenceScore);

                // TODO: Publier l'événement DocumentOcrCompletedEvent
                // TODO: Mettre à jour le statut du document
            }
            else
            {
                _logger.LogError("Échec OCR pour {DocumentId}: {Error}", 
                    notification.DocumentId, result.Error);
                
                // TODO: Publier un événement d'erreur
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement OCR du document {DocumentId}", 
                notification.DocumentId);
        }
    }

    private static bool IsOcrProcessable(string? mimeType)
    {
        if (string.IsNullOrEmpty(mimeType))
            return false;

        return mimeType.StartsWith("image/") || mimeType == "application/pdf";
    }
}
