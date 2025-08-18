using CaptureSys.Shared.Entities;

namespace CaptureSys.AutoLearningService.Domain.Entities;

public class TrainingJob : BaseEntity
{
    public string ModelName { get; private set; } = string.Empty;
    public ModelType ModelType { get; private set; }
    public TrainingStatus Status { get; private set; }
    public string DatasetPath { get; private set; } = string.Empty;
    public string? OutputModelPath { get; private set; }
    public Dictionary<string, object> Parameters { get; private set; } = new();
    public TrainingMetrics? Metrics { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public double? ProgressPercentage { get; private set; }
    public int? EpochsCompleted { get; private set; }
    public int? TotalEpochs { get; private set; }

    private TrainingJob() { } // EF Constructor

    public TrainingJob(string modelName, ModelType modelType, string datasetPath)
    {
        ModelName = modelName;
        ModelType = modelType;
        DatasetPath = datasetPath;
        Status = TrainingStatus.Pending;
        Parameters = new Dictionary<string, object>();
    }

    public void Start()
    {
        Status = TrainingStatus.Training;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(double progressPercentage, int epochsCompleted)
    {
        ProgressPercentage = progressPercentage;
        EpochsCompleted = epochsCompleted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string outputModelPath, TrainingMetrics metrics)
    {
        Status = TrainingStatus.Completed;
        OutputModelPath = outputModelPath;
        Metrics = metrics;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        ProgressPercentage = 100.0;
    }

    public void Fail(string errorMessage)
    {
        Status = TrainingStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddParameter(string key, object value)
    {
        Parameters[key] = value;
    }
}

public enum ModelType
{
    OcrAccuracy = 1,
    DocumentClassification = 2,
    FieldExtraction = 3,
    ImagePreprocessing = 4,
    LanguageDetection = 5
}

public enum TrainingStatus
{
    Pending = 1,
    Training = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public class TrainingMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double Loss { get; set; }
    public int SamplesProcessed { get; set; }
}
