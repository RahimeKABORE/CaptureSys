using System.Text.Json;
using Microsoft.Extensions.Logging;
using CaptureSys.ExportService.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.ExportService.Infrastructure.Exporters;

public class JsonExporter
{
    private readonly ILogger<JsonExporter> _logger;

    public JsonExporter(ILogger<JsonExporter> logger)
    {
        _logger = logger;
    }

    public async Task<Result<string>> ExportAsync(List<ExportedDocument> documents, ExportConfiguration configuration, string outputPath)
    {
        try
        {
            _logger.LogInformation("Début export JSON vers {OutputPath} pour {Count} documents", outputPath, documents.Count);

            var jsonPath = Path.Combine(outputPath, $"export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);

            var exportData = documents.Select(doc => CreateJsonDocument(doc, configuration)).ToList();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(exportData, options);
            await File.WriteAllTextAsync(jsonPath, json);

            _logger.LogInformation("Export JSON terminé: {FilePath}", jsonPath);
            return Result<string>.Success(jsonPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'export JSON");
            return Result<string>.Failure($"Erreur export JSON: {ex.Message}");
        }
    }

    private static object CreateJsonDocument(ExportedDocument document, ExportConfiguration configuration)
    {
        var result = new Dictionary<string, object>
        {
            ["documentId"] = document.OriginalDocumentId,
            ["fileName"] = document.FileName,
            ["documentType"] = document.DocumentType,
            ["exportedAt"] = document.ExportedAt
        };

        // Ajouter les champs configurés ou tous les champs
        var fieldsToInclude = configuration.FieldsToExport.Any() 
            ? configuration.FieldsToExport 
            : document.ExportedFields.Keys.ToList();

        foreach (var fieldName in fieldsToInclude)
        {
            var actualFieldName = configuration.FieldMappings.GetValueOrDefault(fieldName, fieldName);
            if (document.ExportedFields.TryGetValue(actualFieldName, out var value))
            {
                result[fieldName] = value;
            }
        }

        // Ajouter les métadonnées si demandées
        if (configuration.IncludeMetadata && document.Metadata.Any())
        {
            result["metadata"] = document.Metadata;
        }

        return result;
    }
}
