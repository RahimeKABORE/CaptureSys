using CaptureSys.Shared.Results;

namespace CaptureSys.OcrService.Application.Interfaces;

public interface IOcrProcessor
{
    Task<Result<OcrResult>> ProcessDocumentAsync(string filePath, string language = "eng");
    Task<Result<OcrResult>> ProcessImageAsync(Stream imageStream, string language = "eng");
}

public class OcrResult
{
    public string ExtractedText { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public int PageCount { get; set; } = 1;
    public List<OcrWord> Words { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

public class OcrWord
{
    public string Text { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
