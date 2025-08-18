using System.Text;
using Microsoft.Extensions.Logging;
using CaptureSys.ExportService.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.ExportService.Infrastructure.Exporters;

public class CsvExporter
{
    private readonly ILogger<CsvExporter> _logger;

    public CsvExporter(ILogger<CsvExporter> logger)
    {
        _logger = logger;
    }

    public async Task<Result<string>> ExportAsync(List<ExportedDocument> documents, ExportConfiguration configuration, string outputPath)
    {
        try
        {
            _logger.LogInformation("Début export CSV vers {OutputPath} pour {Count} documents", outputPath, documents.Count);

            var csvPath = Path.Combine(outputPath, $"export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(csvPath)!);

            var csvContent = new StringBuilder();

            // Écrire les en-têtes
            var headers = GetHeaders(documents, configuration);
            csvContent.AppendLine(string.Join(",", headers.Select(EscapeCsvField)));

            // Écrire les données
            foreach (var document in documents)
            {
                var row = await CreateDocumentRow(document, configuration, headers);
                csvContent.AppendLine(string.Join(",", row.Select(EscapeCsvField)));
            }

            // Sauvegarder le fichier
            await File.WriteAllTextAsync(csvPath, csvContent.ToString(), Encoding.UTF8);

            _logger.LogInformation("Export CSV terminé: {FilePath}", csvPath);
            return Result<string>.Success(csvPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'export CSV");
            return Result<string>.Failure($"Erreur export CSV: {ex.Message}");
        }
    }

    private static List<string> GetHeaders(List<ExportedDocument> documents, ExportConfiguration configuration)
    {
        var headers = new List<string>();

        // Headers de base
        headers.AddRange(new[] { "DocumentId", "FileName", "DocumentType", "ExportedAt" });

        // Champs configurés ou tous les champs disponibles
        if (configuration.FieldsToExport.Any())
        {
            headers.AddRange(configuration.FieldsToExport);
        }
        else
        {
            var allFields = documents.SelectMany(d => d.ExportedFields.Keys).Distinct().ToList();
            headers.AddRange(allFields);
        }

        // Métadonnées si demandées
        if (configuration.IncludeMetadata)
        {
            var metadataFields = documents.SelectMany(d => d.Metadata.Keys).Distinct().Select(k => $"Meta_{k}").ToList();
            headers.AddRange(metadataFields);
        }

        return headers;
    }

    private static async Task<List<string>> CreateDocumentRow(ExportedDocument document, ExportConfiguration configuration, List<string> headers)
    {
        await Task.Yield();

        var row = new List<string>();

        foreach (var header in headers)
        {
            var value = header switch
            {
                "DocumentId" => document.OriginalDocumentId.ToString(),
                "FileName" => document.FileName,
                "DocumentType" => document.DocumentType,
                "ExportedAt" => document.ExportedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                _ when header.StartsWith("Meta_") => GetMetadataValue(document, header[5..]),
                _ => GetFieldValue(document, header, configuration)
            };

            row.Add(value);
        }

        return row;
    }

    private static string GetFieldValue(ExportedDocument document, string fieldName, ExportConfiguration configuration)
    {
        // Appliquer le mapping si configuré
        var actualFieldName = configuration.FieldMappings.GetValueOrDefault(fieldName, fieldName);
        
        return document.ExportedFields.TryGetValue(actualFieldName, out var value) 
            ? value?.ToString() ?? string.Empty 
            : string.Empty;
    }

    private static string GetMetadataValue(ExportedDocument document, string metadataKey)
    {
        return document.Metadata.TryGetValue(metadataKey, out var value) 
            ? value?.ToString() ?? string.Empty 
            : string.Empty;
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // Échapper les guillemets et entourer de guillemets si nécessaire
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return '"' + field.Replace("\"", "\"\"") + '"';
        }

        return field;
    }
}