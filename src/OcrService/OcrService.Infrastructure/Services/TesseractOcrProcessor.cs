using Tesseract;
using Microsoft.Extensions.Logging;
using CaptureSys.OcrService.Application.Interfaces;
using CaptureSys.Shared.Results;
using System.Diagnostics;

namespace CaptureSys.OcrService.Infrastructure.Services;

public class TesseractOcrProcessor : IOcrProcessor
{
    private readonly ILogger<TesseractOcrProcessor> _logger;
    private readonly string _tessDataPath;

    public TesseractOcrProcessor(ILogger<TesseractOcrProcessor> logger)
    {
        _logger = logger;
        _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
        EnsureTessDataExists();
    }

    public async Task<Result<OcrResult>> ProcessDocumentAsync(string filePath, string language = "eng")
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            if (!File.Exists(filePath))
            {
                return Result<OcrResult>.Failure("Fichier non trouvé");
            }

            _logger.LogInformation("Début du traitement OCR pour: {FilePath}", filePath);

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var result = await ProcessImageAsync(fileStream, language);
            
            stopwatch.Stop();
            if (result.IsSuccess)
            {
                result.Value!.ProcessingTime = stopwatch.Elapsed;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement OCR du fichier {FilePath}", filePath);
            return Result<OcrResult>.Failure($"Erreur OCR: {ex.Message}");
        }
    }

    public async Task<Result<OcrResult>> ProcessImageAsync(Stream imageStream, string language = "eng")
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            _logger.LogDebug("Début de l'extraction de texte avec Tesseract");
            
            // Vérifier que les données de langue existent
            var langDataFile = Path.Combine(_tessDataPath, $"{language}.traineddata");
            if (!File.Exists(langDataFile))
            {
                _logger.LogWarning("Données de langue {Language} manquantes, utilisation de l'anglais par défaut", language);
                language = "eng";
                langDataFile = Path.Combine(_tessDataPath, "eng.traineddata");
                
                if (!File.Exists(langDataFile))
                {
                    return Result<OcrResult>.Failure("Aucune donnée de langue Tesseract disponible. Veuillez installer eng.traineddata");
                }
            }
            
            // Traitement direct avec Tesseract (seulement pour les images)
            using var engine = new TesseractEngine(_tessDataPath, language, EngineMode.Default);
            engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,;:!?()[]{}\"'-+/\\@#$%^&*=<>| \n\r\t");
            
            // Lecture sécurisée du stream
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            
            using var pix = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(pix);
            
            var extractedText = page.GetText().Trim();
            var confidence = page.GetMeanConfidence();
            
            stopwatch.Stop();

            var ocrResult = new OcrResult
            {
                ExtractedText = extractedText,
                ConfidenceScore = confidence * 100, // Convertir en pourcentage
                PageCount = 1,
                ProcessingTime = stopwatch.Elapsed,
                Words = ExtractWords(page)
            };

            _logger.LogInformation("OCR terminé. Texte extrait: {TextLength} caractères, Confiance: {Confidence}%, Durée: {Duration}ms",
                extractedText.Length, ocrResult.ConfidenceScore, stopwatch.ElapsedMilliseconds);

            return Result<OcrResult>.Success(ocrResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement OCR de l'image");
            return Result<OcrResult>.Failure($"Erreur OCR: {ex.Message}");
        }
    }

    private List<OcrWord> ExtractWords(Page page)
    {
        var words = new List<OcrWord>();
        
        try
        {
            using var iterator = page.GetIterator();
            iterator.Begin();

            do
            {
                if (iterator.TryGetBoundingBox(PageIteratorLevel.Word, out var rect))
                {
                    var word = iterator.GetText(PageIteratorLevel.Word)?.Trim();
                    var confidence = iterator.GetConfidence(PageIteratorLevel.Word);

                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        words.Add(new OcrWord
                        {
                            Text = word,
                            Confidence = confidence,
                            X = rect.X1,
                            Y = rect.Y1,
                            Width = rect.Width,
                            Height = rect.Height
                        });
                    }
                }
            } while (iterator.Next(PageIteratorLevel.Word));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de l'extraction des mots individuels");
        }

        return words;
    }

    private void EnsureTessDataExists()
    {
        if (!Directory.Exists(_tessDataPath))
        {
            Directory.CreateDirectory(_tessDataPath);
            _logger.LogWarning("Dossier tessdata créé: {TessDataPath}. Vous devez installer les données de langue Tesseract.", _tessDataPath);
        }
        
        var engDataFile = Path.Combine(_tessDataPath, "eng.traineddata");
        if (!File.Exists(engDataFile))
        {
            _logger.LogWarning("Fichier de données anglaises manquant: {EngDataFile}. OCR peut échouer.", engDataFile);
        }
    }
}
