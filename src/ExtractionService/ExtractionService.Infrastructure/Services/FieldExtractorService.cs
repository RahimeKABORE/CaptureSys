using Microsoft.Extensions.Logging;
using CaptureSys.ExtractionService.Application.Interfaces;
using CaptureSys.ExtractionService.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.ExtractionService.Infrastructure.Services;

public class FieldExtractorService : IFieldExtractor
{
    private readonly ILogger<FieldExtractorService> _logger;
    private readonly IExtractionEngine _extractionEngine;
    private readonly Dictionary<string, List<ExtractionTemplate>> _templates;

    public FieldExtractorService(
        ILogger<FieldExtractorService> logger,
        IExtractionEngine extractionEngine)
    {
        _logger = logger;
        _extractionEngine = extractionEngine;
        _templates = new Dictionary<string, List<ExtractionTemplate>>();
        
        InitializeDefaultTemplates();
    }

    public async Task<Result<ExtractionResult>> ExtractFieldsAsync(Guid documentId, string documentType, string extractedText)
    {
        try
        {
            _logger.LogInformation("Extraction de champs pour document {DocumentId} de type {DocumentType}", 
                documentId, documentType);

            var startTime = DateTime.UtcNow;

            // Récupérer les templates pour ce type de document
            var templatesResult = await GetTemplatesForDocumentTypeAsync(documentType);
            if (templatesResult.IsFailure || templatesResult.Value == null || !templatesResult.Value.Any())
            {
                _logger.LogWarning("Aucun template trouvé pour le type {DocumentType}", documentType);
                return Result<ExtractionResult>.Failure($"Aucun template trouvé pour le type {documentType}");
            }

            // Utiliser le template avec la priorité la plus élevée
            var template = templatesResult.Value.OrderByDescending(t => t.Priority).First();
            
            return await ExtractFieldsWithTemplateAsync(documentId, template.Name, extractedText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction de champs pour le document {DocumentId}", documentId);
            return Result<ExtractionResult>.Failure($"Erreur d'extraction: {ex.Message}");
        }
    }

    public async Task<Result<ExtractionResult>> ExtractFieldsWithTemplateAsync(Guid documentId, string templateName, string extractedText)
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Trouver le template
            var template = _templates.Values.SelectMany(t => t).FirstOrDefault(t => t.Name == templateName);
            if (template == null)
            {
                return Result<ExtractionResult>.Failure($"Template {templateName} non trouvé");
            }

            _logger.LogInformation("Extraction avec template {TemplateName} pour document {DocumentId}", 
                templateName, documentId);

            // Extraire les champs
            var fieldsResult = await _extractionEngine.ExtractFieldsAsync(extractedText, template.FieldRules);
            if (fieldsResult.IsFailure)
            {
                return Result<ExtractionResult>.Failure(fieldsResult.Error ?? "Erreur d'extraction inconnue");
            }

            var processingTime = DateTime.UtcNow - startTime;

            var extractionResult = new ExtractionResult(
                documentId,
                template.DocumentType,
                templateName,
                fieldsResult.Value!,
                processingTime,
                "ExtractionService");

            _logger.LogInformation("Extraction terminée pour document {DocumentId}: {FieldCount} champs extraits en {ProcessingTime}ms",
                documentId, fieldsResult.Value?.Count ?? 0, processingTime.TotalMilliseconds);

            return Result<ExtractionResult>.Success(extractionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction avec template {TemplateName}", templateName);
            return Result<ExtractionResult>.Failure($"Erreur d'extraction: {ex.Message}");
        }
    }

    public async Task<Result<List<ExtractionTemplate>>> GetTemplatesForDocumentTypeAsync(string documentType)
    {
        try
        {
            await Task.Yield();

            if (_templates.TryGetValue(documentType, out var templates))
            {
                var activeTemplates = templates.Where(t => t.IsActive).ToList();
                return Result<List<ExtractionTemplate>>.Success(activeTemplates);
            }

            return Result<List<ExtractionTemplate>>.Success(new List<ExtractionTemplate>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des templates pour {DocumentType}", documentType);
            return Result<List<ExtractionTemplate>>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<ExtractionTemplate>> CreateTemplateAsync(string name, string documentType, string description, List<FieldExtractionRule> rules)
    {
        try
        {
            await Task.Yield();

            var template = new ExtractionTemplate(name, documentType, description, rules);

            if (!_templates.ContainsKey(documentType))
            {
                _templates[documentType] = new List<ExtractionTemplate>();
            }

            _templates[documentType].Add(template);

            _logger.LogInformation("Template {TemplateName} créé pour le type {DocumentType}", name, documentType);

            return Result<ExtractionTemplate>.Success(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du template {TemplateName}", name);
            return Result<ExtractionTemplate>.Failure($"Erreur de création: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateTemplateAsync(Guid templateId, string name, string description, List<FieldExtractionRule> rules)
    {
        try
        {
            await Task.Yield();

            var template = _templates.Values.SelectMany(t => t).FirstOrDefault(t => t.Id == templateId);
            if (template == null)
            {
                return Result<bool>.Failure("Template non trouvé");
            }

            template.UpdateTemplate(name, description, rules);

            _logger.LogInformation("Template {TemplateId} mis à jour", templateId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du template {TemplateId}", templateId);
            return Result<bool>.Failure($"Erreur de mise à jour: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteTemplateAsync(Guid templateId)
    {
        try
        {
            await Task.Yield();

            foreach (var templateList in _templates.Values)
            {
                var template = templateList.FirstOrDefault(t => t.Id == templateId);
                if (template != null)
                {
                    templateList.Remove(template);
                    _logger.LogInformation("Template {TemplateId} supprimé", templateId);
                    return Result<bool>.Success(true);
                }
            }

            return Result<bool>.Failure("Template non trouvé");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du template {TemplateId}", templateId);
            return Result<bool>.Failure($"Erreur de suppression: {ex.Message}");
        }
    }

    public async Task<Result<ExtractionResult>> ValidateExtractionAsync(Guid extractionId, string validatedBy)
    {
        try
        {
            await Task.Yield();

            // Dans une vraie implémentation, on irait chercher en base
            // Pour l'instant, on simule
            _logger.LogInformation("Validation de l'extraction {ExtractionId} par {ValidatedBy}", extractionId, validatedBy);

            return Result<ExtractionResult>.Failure("Extraction non trouvée - implémentation simulation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la validation de l'extraction {ExtractionId}", extractionId);
            return Result<ExtractionResult>.Failure($"Erreur de validation: {ex.Message}");
        }
    }

    private void InitializeDefaultTemplates()
    {
        // Template pour les factures
        var invoiceRules = new List<FieldExtractionRule>
        {
            new("InvoiceNumber", FieldType.Text, ExtractionMethod.Regex, 
                new List<string> { @"(?:facture|invoice)\s*n°?\s*(\w+)", @"n°\s*(\w+)" },
                new List<string> { "numéro", "facture", "invoice", "n°" }, true),
            
            new("TotalAmount", FieldType.Amount, ExtractionMethod.Regex,
                new List<string> { @"total\s*:?\s*(\d+[,\.]\d{2})\s*€?", @"€\s*(\d+[,\.]\d{2})" },
                new List<string> { "total", "montant", "amount" }, true),
            
            new("DueDate", FieldType.Date, ExtractionMethod.Regex,
                new List<string> { @"échéance\s*:?\s*(\d{1,2}[\/\-\.]\d{1,2}[\/\-\.]\d{2,4})" },
                new List<string> { "échéance", "due date", "payable" }),
            
            new("VatAmount", FieldType.Amount, ExtractionMethod.Regex,
                new List<string> { @"tva\s*:?\s*(\d+[,\.]\d{2})\s*€?" },
                new List<string> { "tva", "vat", "taxe" })
        };

        var invoiceTemplate = new ExtractionTemplate("Invoice_Default", "Invoice", "Template par défaut pour les factures", invoiceRules);

        // Template pour les contrats
        var contractRules = new List<FieldExtractionRule>
        {
            new("ContractNumber", FieldType.Text, ExtractionMethod.Regex,
                new List<string> { @"contrat\s*n°?\s*(\w+)" },
                new List<string> { "contrat", "contract", "n°" }),
            
            new("SigningDate", FieldType.Date, ExtractionMethod.Regex,
                new List<string> { @"signé\s*le\s*(\d{1,2}[\/\-\.]\d{1,2}[\/\-\.]\d{2,4})" },
                new List<string> { "signé", "signature", "date" }),
            
            new("Duration", FieldType.Text, ExtractionMethod.Keyword,
                new List<string>(),
                new List<string> { "durée", "période", "mois", "années" })
        };

        var contractTemplate = new ExtractionTemplate("Contract_Default", "Contract", "Template par défaut pour les contrats", contractRules);

        // Ajouter les templates
        _templates["Invoice"] = new List<ExtractionTemplate> { invoiceTemplate };
        _templates["Contract"] = new List<ExtractionTemplate> { contractTemplate };

        _logger.LogInformation("Templates par défaut initialisés: {TemplateCount} templates chargés", 
            _templates.Values.Sum(t => t.Count));
    }
}
