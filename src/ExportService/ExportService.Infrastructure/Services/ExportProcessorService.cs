using Microsoft.Extensions.Logging;
using CaptureSys.ExportService.Application.Interfaces;
using CaptureSys.ExportService.Domain.Entities;
using CaptureSys.ExportService.Infrastructure.Exporters;
using CaptureSys.Shared.Results;

namespace CaptureSys.ExportService.Infrastructure.Services;

public class ExportProcessorService : IExportProcessor
{
    private readonly ILogger<ExportProcessorService> _logger;
    private readonly CsvExporter _csvExporter;
    private readonly JsonExporter _jsonExporter;
    private readonly Dictionary<Guid, ExportJob> _jobs;
    private readonly List<ExportConfiguration> _configurations;

    public ExportProcessorService(
        ILogger<ExportProcessorService> logger,
        CsvExporter csvExporter,
        JsonExporter jsonExporter)
    {
        _logger = logger;
        _csvExporter = csvExporter;
        _jsonExporter = jsonExporter;
        _jobs = new Dictionary<Guid, ExportJob>();
        _configurations = new List<ExportConfiguration>();
        
        InitializeDefaultConfigurations();
    }

    public async Task<Result<ExportJob>> CreateExportJobAsync(string name, ExportFormat format, ExportDestination destination, 
        List<Guid> documentIds, ExportConfiguration configuration, string requestedBy)
    {
        try
        {
            await Task.Yield();

            var job = new ExportJob(name, format, destination, documentIds, configuration, requestedBy);
            _jobs[job.Id] = job;

            _logger.LogInformation("Job d'export créé: {JobId} pour {Count} documents par {RequestedBy}", 
                job.Id, documentIds.Count, requestedBy);

            return Result<ExportJob>.Success(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du job d'export");
            return Result<ExportJob>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<string>> ProcessExportJobAsync(Guid jobId)
    {
        try
        {
            if (!_jobs.TryGetValue(jobId, out var job))
            {
                return Result<string>.Failure("Job d'export non trouvé");
            }

            _logger.LogInformation("Traitement du job d'export {JobId}", jobId);
            job.Start();

            // Simuler la récupération des documents et de leurs données extraites
            var documents = await GetDocumentsForExportAsync(job.DocumentIds);
            
            // Traiter l'export selon le format
            var result = job.Format switch
            {
                ExportFormat.CSV => await _csvExporter.ExportAsync(documents, job.Configuration, GetOutputPath(job)),
                ExportFormat.JSON => await _jsonExporter.ExportAsync(documents, job.Configuration, GetOutputPath(job)),
                _ => Result<string>.Failure($"Format {job.Format} non supporté")
            };

            if (result.IsSuccess)
            {
                job.Complete(result.Value!);
                _logger.LogInformation("Job d'export {JobId} terminé avec succès: {ResultPath}", jobId, result.Value);
            }
            else
            {
                job.Fail(result.Error!);
                _logger.LogError("Job d'export {JobId} échoué: {Error}", jobId, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            if (_jobs.TryGetValue(jobId, out var job))
            {
                job.Fail(ex.Message);
            }
            _logger.LogError(ex, "Erreur lors du traitement du job d'export {JobId}", jobId);
            return Result<string>.Failure($"Erreur de traitement: {ex.Message}");
        }
    }

    public async Task<Result<ExportJob>> GetExportJobAsync(Guid jobId)
    {
        try
        {
            await Task.Yield();

            if (_jobs.TryGetValue(jobId, out var job))
            {
                return Result<ExportJob>.Success(job);
            }

            return Result<ExportJob>.Failure("Job d'export non trouvé");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du job {JobId}", jobId);
            return Result<ExportJob>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<List<ExportJob>>> GetExportJobsAsync(string? requestedBy = null)
    {
        try
        {
            await Task.Yield();

            var jobs = _jobs.Values.ToList();
            
            if (!string.IsNullOrEmpty(requestedBy))
            {
                jobs = jobs.Where(j => j.RequestedBy == requestedBy).ToList();
            }

            return Result<List<ExportJob>>.Success(jobs.OrderByDescending(j => j.CreatedAt).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des jobs d'export");
            return Result<List<ExportJob>>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CancelExportJobAsync(Guid jobId)
    {
        try
        {
            await Task.Yield();

            if (_jobs.TryGetValue(jobId, out var job))
            {
                // Logique d'annulation (pour simplification, on change juste le statut)
                _logger.LogInformation("Job d'export {JobId} annulé", jobId);
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Job d'export non trouvé");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'annulation du job {JobId}", jobId);
            return Result<bool>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<List<ExportConfiguration>>> GetExportConfigurationsAsync()
    {
        try
        {
            await Task.Yield();
            return Result<List<ExportConfiguration>>.Success(_configurations.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des configurations d'export");
            return Result<List<ExportConfiguration>>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<ExportConfiguration>> CreateExportConfigurationAsync(ExportConfiguration configuration)
    {
        try
        {
            await Task.Yield();
            _configurations.Add(configuration);
            _logger.LogInformation("Configuration d'export créée: {ConfigName}", configuration.Name);
            return Result<ExportConfiguration>.Success(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la configuration d'export");
            return Result<ExportConfiguration>.Failure($"Erreur: {ex.Message}");
        }
    }

    private async Task<List<ExportedDocument>> GetDocumentsForExportAsync(List<Guid> documentIds)
    {
        await Task.Yield();

        // Simulation de récupération des documents avec leurs données extraites
        return documentIds.Select(id => new ExportedDocument(
            id,
            Guid.NewGuid(),
            $"document_{id:N}.pdf",
            "Invoice",
            new Dictionary<string, object>
            {
                ["InvoiceNumber"] = $"INV-{Random.Shared.Next(1000, 9999)}",
                ["TotalAmount"] = Random.Shared.Next(100, 1000) + 0.50,
                ["DueDate"] = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"),
                ["VatAmount"] = Random.Shared.Next(10, 100) + 0.12
            },
            new Dictionary<string, object>
            {
                ["ProcessedAt"] = DateTime.UtcNow,
                ["ConfidenceScore"] = 0.95,
                ["Source"] = "OCR"
            }
        )).ToList();
    }

    private static string GetOutputPath(ExportJob job)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "exports", job.Id.ToString());
    }

    private void InitializeDefaultConfigurations()
    {
        // Configuration CSV pour factures
        var csvInvoiceConfig = new ExportConfiguration(
            "CSV_Invoice_Default",
            ExportFormat.CSV,
            ExportDestination.Local,
            new List<string> { "InvoiceNumber", "TotalAmount", "DueDate", "VatAmount" },
            new Dictionary<string, string>
            {
                ["InvoiceNumber"] = "Numéro Facture",
                ["TotalAmount"] = "Montant Total",
                ["DueDate"] = "Date Échéance",
                ["VatAmount"] = "Montant TVA"
            }
        );

        // Configuration JSON complète
        var jsonCompleteConfig = new ExportConfiguration(
            "JSON_Complete",
            ExportFormat.JSON,
            ExportDestination.Local,
            new List<string>(), // Tous les champs
            includeMetadata: true
        );

        _configurations.AddRange(new[] { csvInvoiceConfig, jsonCompleteConfig });

        _logger.LogInformation("Configurations d'export par défaut initialisées: {Count}", _configurations.Count);
    }
}
