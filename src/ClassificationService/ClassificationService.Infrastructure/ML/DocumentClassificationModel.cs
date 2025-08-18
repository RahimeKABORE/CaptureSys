using Microsoft.ML.Data;

namespace CaptureSys.ClassificationService.Infrastructure.ML;

public class DocumentClassificationInput
{
    [LoadColumn(0), ColumnName("Text")]
    public string Text { get; set; } = string.Empty;

    [LoadColumn(1), ColumnName("Label")]
    public string DocumentType { get; set; } = string.Empty;
}

public class DocumentClassificationOutput
{
    [ColumnName("PredictedLabel")]
    public string PredictedDocumentType { get; set; } = string.Empty;

    [ColumnName("Score")]
    public float[] Score { get; set; } = Array.Empty<float>();

    [ColumnName("Probability")]
    public float[] Probability { get; set; } = Array.Empty<float>();
}
