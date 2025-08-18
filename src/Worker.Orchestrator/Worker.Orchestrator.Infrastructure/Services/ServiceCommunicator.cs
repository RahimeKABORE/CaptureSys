using Microsoft.Extensions.Logging;
using CaptureSys.Shared.Results;

namespace CaptureSys.Worker.Orchestrator.Infrastructure.Services;

public interface IServiceCommunicator
{
    Task<Result<string>> CheckIngestionStatusAsync(string documentId);
    Task<Result<string>> ProcessOcrAsync(string documentId);
    Task<Result<string>> ClassifyDocumentAsync(string documentId);
    Task<Result<string>> ExtractDataAsync(string documentId);
    Task<Result<string>> ExportDataAsync(string documentId);
}

public class ServiceCommunicator : IServiceCommunicator
{
    private readonly ILogger<ServiceCommunicator> _logger;

    public ServiceCommunicator(ILogger<ServiceCommunicator> logger)
    {
        _logger = logger;
    }

    public async Task<Result<string>> CheckIngestionStatusAsync(string documentId)
    {
        await Task.Delay(500); // Simulation
        _logger.LogInformation("Vérification ingestion pour {DocumentId}", documentId);
        return Result<string>.Success($"Document {documentId} ingéré");
    }

    public async Task<Result<string>> ProcessOcrAsync(string documentId)
    {
        await Task.Delay(1000); // Simulation OCR
        _logger.LogInformation("OCR traité pour {DocumentId}", documentId);
        return Result<string>.Success($"OCR terminé pour {documentId}");
    }

    public async Task<Result<string>> ClassifyDocumentAsync(string documentId)
    {
        await Task.Delay(800); // Simulation classification
        _logger.LogInformation("Classification pour {DocumentId}", documentId);
        return Result<string>.Success($"Document {documentId} classifié comme Facture");
    }

    public async Task<Result<string>> ExtractDataAsync(string documentId)
    {
        await Task.Delay(1200); // Simulation extraction
        _logger.LogInformation("Extraction pour {DocumentId}", documentId);
        return Result<string>.Success($"Données extraites de {documentId}");
    }

    public async Task<Result<string>> ExportDataAsync(string documentId)
    {
        await Task.Delay(600); // Simulation export
        _logger.LogInformation("Export pour {DocumentId}", documentId);
        return Result<string>.Success($"Document {documentId} exporté");
    }
}
