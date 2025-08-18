using CaptureSys.Shared.Entities;

namespace CaptureSys.ImageProcessorService.Domain.Entities;

public class ImageProcessingJob : BaseEntity
{
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public ImageOperation Operation { get; private set; }
    public JobStatus Status { get; private set; }
    public string? OutputPath { get; private set; }
    public Dictionary<string, object> Parameters { get; private set; } = new();
    public string? ErrorMessage { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long? InputSize { get; private set; }
    public long? OutputSize { get; private set; }
    public double? ProcessingTimeMs { get; private set; }

    private ImageProcessingJob() { } // EF Constructor

    public ImageProcessingJob(string fileName, string filePath, ImageOperation operation, long inputSize)
    {
        FileName = fileName;
        FilePath = filePath;
        Operation = operation;
        Status = JobStatus.Pending;
        InputSize = inputSize;
    }

    public void Start()
    {
        Status = JobStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string outputPath, long outputSize, double processingTimeMs)
    {
        Status = JobStatus.Completed;
        OutputPath = outputPath;
        OutputSize = outputSize;
        ProcessingTimeMs = processingTimeMs;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = JobStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddParameter(string key, object value)
    {
        Parameters[key] = value;
    }
}

public enum ImageOperation
{
    Deskew = 1,
    Denoise = 2,
    Binarize = 3,
    Resize = 4,
    Rotate = 5,
    Crop = 6,
    Enhance = 7,
    Normalize = 8
}

public enum JobStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
