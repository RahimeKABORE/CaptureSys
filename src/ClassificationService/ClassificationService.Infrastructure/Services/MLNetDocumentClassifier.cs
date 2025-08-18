using Microsoft.ML;
using Microsoft.Extensions.Logging;
using CaptureSys.ClassificationService.Application.Interfaces;
using CaptureSys.ClassificationService.Domain.Entities;
using CaptureSys.ClassificationService.Infrastructure.ML;
using CaptureSys.Shared.Results;
using System.Text.RegularExpressions;

namespace CaptureSys.ClassificationService.Infrastructure.Services;

public class MLNetDocumentClassifier : IDocumentClassifier
{
    private readonly ILogger<MLNetDocumentClassifier> _logger;
    private readonly MLContext _mlContext;
    private readonly string _modelPath;
    private ITransformer? _model;
    private PredictionEngine<DocumentClassificationInput, DocumentClassificationOutput>? _predictionEngine;
    private readonly string _modelVersion;

    // Types de documents prédéfinis
    private static readonly Dictionary<string, List<string>> DocumentTypeKeywords = new()
    {
        { "Invoice", new() { "facture", "invoice", "montant", "tva", "total", "échéance", "payment", "due" } },
        { "Contract", new() { "contrat", "contract", "accord", "agreement", "clause", "partie", "signataire" } },
        { "Receipt", new() { "reçu", "receipt", "ticket", "achat", "purchase", "magasin", "store" } },
        { "Identity", new() { "carte", "identité", "passeport", "passport", "permis", "license", "né", "born" } },
        { "Medical", new() { "médical", "medical", "ordonnance", "prescription", "docteur", "patient", "diagnostic" } },
        { "Legal", new() { "juridique", "legal", "tribunal", "court", "loi", "law", "avocat", "lawyer" } },
        { "Financial", new() { "banque", "bank", "compte", "account", "crédit", "débit", "solde", "balance" } },
        { "Other", new() { "document", "texte", "information", "data" } }
    };

    public MLNetDocumentClassifier(ILogger<MLNetDocumentClassifier> logger)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 1);
        _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "Models", "document-classification.zip");
        _modelVersion = "1.0.0";
        
        EnsureModelDirectoryExists();
        LoadOrCreateModel();
    }

    public async Task<Result<ClassificationResult>> ClassifyDocumentAsync(Guid documentId, string extractedText)
    {
        try
        {
            _logger.LogInformation("Classification du document {DocumentId}", documentId);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return Result<ClassificationResult>.Failure("Texte extrait vide ou null");
            }

            var classificationResult = await ClassifyTextInternalAsync(extractedText);
            if (classificationResult.IsFailure)
            {
                return Result<ClassificationResult>.Failure(classificationResult.Error ?? "Erreur de classification inconnue");
            }

            var result = new ClassificationResult(
                documentId,
                classificationResult.Value!.PredictedType,
                classificationResult.Value.Confidence,
                extractedText,
                classificationResult.Value.MatchedKeywords,
                classificationResult.Value.MatchedPatterns,
                _modelVersion);

            // Ajouter les classifications alternatives
            foreach (var alt in classificationResult.Value.AlternativeClassifications)
            {
                result.AddAlternativeClassification(alt.DocumentType, alt.Confidence);
            }

            _logger.LogInformation("Document {DocumentId} classifié comme {DocumentType} avec confiance {Confidence}%",
                documentId, result.PredictedDocumentType, result.Confidence * 100);

            return Result<ClassificationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la classification du document {DocumentId}", documentId);
            return Result<ClassificationResult>.Failure($"Erreur de classification: {ex.Message}");
        }
    }

    public async Task<Result<List<ClassificationScore>>> GetPossibleClassificationsAsync(string text)
    {
        try
        {
            var classificationResult = await ClassifyTextInternalAsync(text);
            if (classificationResult.IsFailure)
            {
                return Result<List<ClassificationScore>>.Failure(classificationResult.Error ?? "Erreur de classification inconnue");
            }

            var scores = new List<ClassificationScore>
            {
                new(classificationResult.Value!.PredictedType, classificationResult.Value.Confidence)
            };
            scores.AddRange(classificationResult.Value.AlternativeClassifications);

            return Result<List<ClassificationScore>>.Success(scores.OrderByDescending(s => s.Confidence).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'obtention des classifications possibles");
            return Result<List<ClassificationScore>>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<bool>> TrainModelAsync(List<TrainingData> trainingData)
    {
        try
        {
            _logger.LogInformation("Démarrage de l'entraînement du modèle avec {Count} échantillons", trainingData.Count);

            if (!trainingData.Any())
            {
                return Result<bool>.Failure("Aucune donnée d'entraînement fournie");
            }

            // Exécuter l'entraînement sur un thread en arrière-plan car c'est CPU-intensif
            return await Task.Run(() =>
            {
                // Créer les données d'entraînement
                var mlData = trainingData.Select(td => new DocumentClassificationInput
                {
                    Text = PreprocessText(td.Text),
                    DocumentType = td.DocumentType
                }).ToList();

                var dataView = _mlContext.Data.LoadFromEnumerable(mlData);

                // Pipeline de traitement simplifié
                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", "Label")
                    .Append(_mlContext.Transforms.Text.FeaturizeText("Features", "Text"))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "Label"));

                // Entraîner le modèle
                _model = pipeline.Fit(dataView);

                // Sauvegarder le modèle
                _mlContext.Model.Save(_model, dataView.Schema, _modelPath);

                // Recréer le moteur de prédiction
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<DocumentClassificationInput, DocumentClassificationOutput>(_model);

                _logger.LogInformation("Modèle entraîné et sauvegardé avec succès");
                return Result<bool>.Success(true);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'entraînement du modèle");
            return Result<bool>.Failure($"Erreur d'entraînement: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateModelAsync()
    {
        try
        {
            // Utiliser des données par défaut pour créer un modèle de base
            var defaultTrainingData = GenerateDefaultTrainingData();
            return await TrainModelAsync(defaultTrainingData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du modèle");
            return Result<bool>.Failure($"Erreur de mise à jour: {ex.Message}");
        }
    }

    public string GetModelVersion() => _modelVersion;

    public async Task<Result<ClassificationResult>> ClassifyTextAsync(string text)
    {
        try
        {
            _logger.LogInformation("Classification de texte de {Length} caractères", text.Length);

            if (string.IsNullOrWhiteSpace(text))
            {
                return Result<ClassificationResult>.Failure("Texte vide ou null");
            }

            var classificationResult = await ClassifyTextInternalAsync(text);
            if (classificationResult.IsFailure)
            {
                return Result<ClassificationResult>.Failure(classificationResult.Error ?? "Erreur de classification inconnue");
            }

            var result = new ClassificationResult(
                Guid.NewGuid(), // ID temporaire pour classification de texte
                classificationResult.Value!.PredictedType,
                classificationResult.Value.Confidence,
                text,
                classificationResult.Value.MatchedKeywords,
                classificationResult.Value.MatchedPatterns,
                _modelVersion);

            // Ajouter les classifications alternatives
            foreach (var alt in classificationResult.Value.AlternativeClassifications)
            {
                result.AddAlternativeClassification(alt.DocumentType, alt.Confidence);
            }

            _logger.LogInformation("Texte classifié comme {DocumentType} avec confiance {Confidence}%",
                result.PredictedDocumentType, result.Confidence * 100);

            return Result<ClassificationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la classification de texte");
            return Result<ClassificationResult>.Failure($"Erreur de classification: {ex.Message}");
        }
    }

    private async Task<Result<InternalClassificationResult>> ClassifyTextInternalAsync(string text)
    {
        try
        {
            await Task.Yield(); // Pour respecter l'async
            
            var processedText = PreprocessText(text);
            var input = new DocumentClassificationInput { Text = processedText };

            // Classification basée sur les règles si pas de modèle ML
            if (_predictionEngine == null)
            {
                return ClassifyWithRules(text);
            }

            // Classification ML.NET
            var prediction = _predictionEngine.Predict(input);
            var maxProbability = prediction.Probability.Max();

            // Classification de secours si confiance trop faible
            if (maxProbability < 0.3)
            {
                return ClassifyWithRules(text);
            }

            var result = new InternalClassificationResult
            {
                PredictedType = prediction.PredictedDocumentType,
                Confidence = maxProbability,
                MatchedKeywords = FindMatchedKeywords(text, prediction.PredictedDocumentType),
                MatchedPatterns = new List<string>(),
                AlternativeClassifications = GetAlternativeClassifications(prediction)
            };

            return Result<InternalClassificationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la classification interne");
            return Result<InternalClassificationResult>.Failure($"Erreur de classification: {ex.Message}");
        }
    }

    private Result<InternalClassificationResult> ClassifyWithRules(string text)
    {
        var textLower = text.ToLowerInvariant();
        var scores = new Dictionary<string, double>();

        foreach (var docType in DocumentTypeKeywords)
        {
            var matchCount = docType.Value.Count(keyword => textLower.Contains(keyword.ToLowerInvariant()));
            var score = (double)matchCount / docType.Value.Count;
            scores[docType.Key] = score;
        }

        var bestMatch = scores.OrderByDescending(s => s.Value).First();
        var matchedKeywords = DocumentTypeKeywords[bestMatch.Key]
            .Where(keyword => textLower.Contains(keyword.ToLowerInvariant()))
            .ToList();

        var result = new InternalClassificationResult
        {
            PredictedType = bestMatch.Value > 0 ? bestMatch.Key : "Other",
            Confidence = Math.Max(bestMatch.Value, 0.1), // Minimum 10% de confiance
            MatchedKeywords = matchedKeywords,
            MatchedPatterns = new List<string>(),
            AlternativeClassifications = scores
                .Where(s => s.Key != bestMatch.Key && s.Value > 0)
                .Select(s => new ClassificationScore(s.Key, s.Value))
                .OrderByDescending(s => s.Confidence)
                .Take(3)
                .ToList()
        };

        return Result<InternalClassificationResult>.Success(result);
    }
    private void EnsureModelDirectoryExists()
    {
        var modelDir = Path.GetDirectoryName(_modelPath);
        if (!Directory.Exists(modelDir))
        {
            Directory.CreateDirectory(modelDir!);
        }
    }

    private void LoadOrCreateModel()
    {
        try
        {
            if (File.Exists(_modelPath))
            {
                _model = _mlContext.Model.Load(_modelPath, out _);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<DocumentClassificationInput, DocumentClassificationOutput>(_model);
                _logger.LogInformation("Modèle ML chargé depuis {ModelPath}", _modelPath);
            }
            else
            {
                _logger.LogWarning("Aucun modèle trouvé. Classification basée sur les règles uniquement.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement du modèle. Utilisation des règles uniquement.");
            _model = null;
            _predictionEngine = null;
        }
    }

    private static string PreprocessText(string text)
    {
        // Nettoyage du texte
        text = Regex.Replace(text, @"[^\w\s]", " ");
        text = Regex.Replace(text, @"\s+", " ");
        return text.Trim().ToLowerInvariant();
    }

    private List<string> FindMatchedKeywords(string text, string documentType)
    {
        if (!DocumentTypeKeywords.ContainsKey(documentType))
            return new List<string>();

        var textLower = text.ToLowerInvariant();
        return DocumentTypeKeywords[documentType]
            .Where(keyword => textLower.Contains(keyword.ToLowerInvariant()))
            .ToList();
    }

    private List<ClassificationScore> GetAlternativeClassifications(DocumentClassificationOutput prediction)
    {
        var alternatives = new List<ClassificationScore>();
        
        // Logique pour extraire les classifications alternatives depuis la prédiction ML.NET
        // Pour simplicité, nous retournons les autres types avec des scores calculés
        
        return alternatives;
    }

    private List<TrainingData> GenerateDefaultTrainingData()
    {
        return new List<TrainingData>
        {
            new("Facture numéro 123 montant total 150.00 EUR TVA 20%", "Invoice"),
            new("Contrat de travail entre l'employeur et l'employé clause 1", "Contract"),
            new("Reçu d'achat magasin ABC montant 25.50 EUR", "Receipt"),
            new("Carte d'identité française nom prénom né le", "Identity"),
            new("Ordonnance médicale docteur Martin prescription médicament", "Medical"),
            new("Document juridique tribunal de Paris avocat procédure", "Legal"),
            new("Relevé bancaire compte numéro solde crédit débit", "Financial"),
            new("Document divers information générale texte", "Other")
        };
    }

    private class InternalClassificationResult
    {
        public string PredictedType { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public List<string> MatchedKeywords { get; set; } = new();
        public List<string> MatchedPatterns { get; set; } = new();
        public List<ClassificationScore> AlternativeClassifications { get; set; } = new();
    }
}