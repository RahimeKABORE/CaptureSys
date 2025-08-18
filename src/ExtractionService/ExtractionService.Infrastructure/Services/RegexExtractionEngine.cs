using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CaptureSys.ExtractionService.Application.Interfaces;
using CaptureSys.ExtractionService.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.ExtractionService.Infrastructure.Services;

public class RegexExtractionEngine : IExtractionEngine
{
    private readonly ILogger<RegexExtractionEngine> _logger;

    // Patterns prédéfinis pour différents types de champs
    private static readonly Dictionary<FieldType, List<string>> DefaultPatterns = new()
    {
        { FieldType.Date, new List<string>
            {
                @"\b\d{1,2}[\/\-\.]\d{1,2}[\/\-\.]\d{2,4}\b", // DD/MM/YYYY, DD-MM-YYYY
                @"\b\d{1,2}\s+(janvier|février|mars|avril|mai|juin|juillet|août|septembre|octobre|novembre|décembre)\s+\d{4}\b",
                @"\b\d{4}[\/\-\.]\d{1,2}[\/\-\.]\d{1,2}\b" // YYYY/MM/DD
            }
        },
        { FieldType.Amount, new List<string>
            {
                @"\b\d+[,\.]\d{2}\s*€?\b", // 123,45 €
                @"\b\d+\s*€\b", // 123 €
                @"€\s*\d+[,\.]\d{2}\b", // € 123,45
                @"\b\d+[,\.]\d{2}\s*(EUR|euro)\b" // 123,45 EUR
            }
        },
        { FieldType.Email, new List<string>
            {
                @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b"
            }
        },
        { FieldType.Phone, new List<string>
            {
                @"\b0[1-9](?:[0-9]{8})\b", // Format français
                @"\b(?:\+33|0)[1-9](?:[0-9]{8})\b",
                @"\b\d{2}\.\d{2}\.\d{2}\.\d{2}\.\d{2}\b" // Format avec points
            }
        },
        { FieldType.Number, new List<string>
            {
                @"\b\d+\b",
                @"\b\d+[,\.]\d+\b"
            }
        }
    };

    public RegexExtractionEngine(ILogger<RegexExtractionEngine> logger)
    {
        _logger = logger;
    }

    public async Task<Result<ExtractedField>> ExtractFieldAsync(string text, FieldExtractionRule rule)
    {
        try
        {
            await Task.Yield(); // Pour l'async

            var extractedField = new ExtractedField(rule.FieldName, rule.FieldType, rule.Method, rule.IsRequired);

            // Extraction selon la méthode
            string? extractedValue = rule.Method switch
            {
                ExtractionMethod.Regex => await ExtractByRegexInternalAsync(text, rule.Patterns),
                ExtractionMethod.Keyword => await ExtractByKeywordInternalAsync(text, rule.Keywords),
                ExtractionMethod.Position => rule.SearchArea != null 
                    ? await ExtractByPositionInternalAsync(text, rule.SearchArea) 
                    : null,
                _ => null
            };

            if (string.IsNullOrEmpty(extractedValue))
            {
                extractedValue = rule.DefaultValue;
            }

            if (!string.IsNullOrEmpty(extractedValue))
            {
                var confidence = await CalculateConfidenceAsync(extractedValue, rule);
                extractedField.SetValue(extractedValue, extractedValue, confidence.Value);
                
                _logger.LogInformation("Champ {FieldName} extrait: {Value} (confiance: {Confidence}%)",
                    rule.FieldName, extractedValue, confidence.Value * 100);
            }
            else if (rule.IsRequired)
            {
                extractedField.SetValidationError($"Champ requis {rule.FieldName} non trouvé");
                _logger.LogWarning("Champ requis {FieldName} non trouvé", rule.FieldName);
            }

            return Result<ExtractedField>.Success(extractedField);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction du champ {FieldName}", rule.FieldName);
            return Result<ExtractedField>.Failure($"Erreur d'extraction: {ex.Message}");
        }
    }

    public async Task<Result<List<ExtractedField>>> ExtractFieldsAsync(string text, List<FieldExtractionRule> rules)
    {
        try
        {
            var extractedFields = new List<ExtractedField>();

            foreach (var rule in rules)
            {
                var fieldResult = await ExtractFieldAsync(text, rule);
                if (fieldResult.IsSuccess)
                {
                    extractedFields.Add(fieldResult.Value!);
                }
                else
                {
                    _logger.LogWarning("Échec extraction champ {FieldName}: {Error}", rule.FieldName, fieldResult.Error);
                }
            }

            return Result<List<ExtractedField>>.Success(extractedFields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction multiple de champs");
            return Result<List<ExtractedField>>.Failure($"Erreur d'extraction multiple: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExtractByRegexAsync(string text, string pattern)
    {
        try
        {
            await Task.Yield();
            var result = await ExtractByRegexInternalAsync(text, new List<string> { pattern });
            return result != null 
                ? Result<string>.Success(result) 
                : Result<string>.Failure("Aucune correspondance trouvée");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Erreur regex: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExtractByKeywordAsync(string text, List<string> keywords)
    {
        try
        {
            await Task.Yield();
            var result = await ExtractByKeywordInternalAsync(text, keywords);
            return result != null 
                ? Result<string>.Success(result) 
                : Result<string>.Failure("Aucun mot-clé trouvé");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Erreur keyword: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExtractByPositionAsync(string text, BoundingBox searchArea)
    {
        try
        {
            await Task.Yield();
            var result = await ExtractByPositionInternalAsync(text, searchArea);
            return result != null 
                ? Result<string>.Success(result) 
                : Result<string>.Failure("Aucun texte trouvé dans la zone");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Erreur position: {ex.Message}");
        }
    }

    public async Task<Result<double>> CalculateConfidenceAsync(string extractedValue, FieldExtractionRule rule)
    {
        try
        {
            await Task.Yield();

            double confidence = 0.5; // Base confidence

            // Augmenter la confiance selon le type de validation
            if (ValidateFieldType(extractedValue, rule.FieldType))
            {
                confidence += 0.3;
            }

            // Augmenter si le pattern correspond exactement
            if (rule.Patterns.Any() && rule.Patterns.Any(p => Regex.IsMatch(extractedValue, p, RegexOptions.IgnoreCase)))
            {
                confidence += 0.2;
            }

            return Result<double>.Success(Math.Min(confidence, 1.0));
        }
        catch
        {
            return Result<double>.Success(0.1); // Confiance minimale
        }
    }

    private async Task<string?> ExtractByRegexInternalAsync(string text, List<string> patterns)
    {
        await Task.Yield();

        foreach (var pattern in patterns)
        {
            try
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (match.Success)
                {
                    return match.Value.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Pattern regex invalide {Pattern}: {Error}", pattern, ex.Message);
            }
        }

        return null;
    }

    private async Task<string?> ExtractByKeywordInternalAsync(string text, List<string> keywords)
    {
        await Task.Yield();

        foreach (var keyword in keywords)
        {
            var index = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                // Extraire le texte autour du mot-clé
                var startIndex = Math.Max(0, index + keyword.Length);
                var endIndex = Math.Min(text.Length, startIndex + 50); // 50 caractères après le mot-clé
                
                var extracted = text.Substring(startIndex, endIndex - startIndex).Trim();
                
                // Nettoyer et extraire la première "valeur" (mot, nombre, etc.)
                var words = extracted.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 0)
                {
                    return words[0];
                }
            }
        }

        return null;
    }

    private async Task<string?> ExtractByPositionInternalAsync(string text, BoundingBox searchArea)
    {
        await Task.Yield();
        
        // Pour l'instant, simulation basique de l'extraction par position
        // Dans une vraie implémentation, on utiliserait les coordonnées OCR
        var lines = text.Split('\n');
        if (searchArea.PageNumber < lines.Length)
        {
            return lines[searchArea.PageNumber]?.Trim();
        }

        return null;
    }

    private static bool ValidateFieldType(string value, FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Date => DateTime.TryParse(value, out _),
            FieldType.Number => double.TryParse(value.Replace(",", "."), out _),
            FieldType.Amount => Regex.IsMatch(value, @"\d+[,\.]\d{2}"),
            FieldType.Email => Regex.IsMatch(value, @"^[^@]+@[^@]+\.[^@]+$"),
            FieldType.Phone => Regex.IsMatch(value, @"^\+?[\d\s\.\-\(\)]+$"),
            FieldType.Boolean => bool.TryParse(value, out _),
            _ => true // Text et Custom sont toujours valides
        };
    }
}
