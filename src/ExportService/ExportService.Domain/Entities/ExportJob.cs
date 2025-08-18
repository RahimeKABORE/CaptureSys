using CaptureSys.Shared.Entities;

namespace CaptureSys.ExportService.Domain.Entities;

public class ExportJob : BaseEntity
{
    public string Name { get; private set; }
    public ExportFormat Format { get; private set; }
    public ExportDestination Destination { get; private set; }
    public ExportStatus Status { get; private set; }
    public List<Guid> DocumentIds { get; private set; }
    public ExportConfiguration Configuration { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ResultPath { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int TotalDocuments { get; private set; }
    public int ProcessedDocuments { get; private set; }
    public string RequestedBy { get; private set; }

    public ExportJob(
        string name,
        ExportFormat format,
        ExportDestination destination,
        List<Guid> documentIds,
        ExportConfiguration configuration,
        string requestedBy)
    {
        Name = name;
        Format = format;
        Destination = destination;
        DocumentIds = documentIds ?? new List<Guid>();
        Configuration = configuration;
        RequestedBy = requestedBy;
        Status = ExportStatus.Pending;
        TotalDocuments = DocumentIds.Count;
        ProcessedDocuments = 0;
    }

    public void Start()
    {
        Status = ExportStatus.Processing;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(string resultPath)
    {
        Status = ExportStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ResultPath = resultPath;
        ProcessedDocuments = TotalDocuments;
    }

    public void Fail(string errorMessage)
    {
        Status = ExportStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    public void UpdateProgress(int processedCount)
    {
        ProcessedDocuments = Math.Min(processedCount, TotalDocuments);
    }

    public double GetProgressPercentage()
    {
        return TotalDocuments > 0 ? (double)ProcessedDocuments / TotalDocuments * 100 : 0;
    }
}

public enum ExportFormat
{
    CSV = 1,
    JSON = 2,
    XML = 3,
    Excel = 4,
    PDF = 5,
    Database = 6
}

public enum ExportDestination
{
    Local = 1,
    PostgreSQL = 2,
    S3 = 3,
    FTP = 4,
    Email = 5,
    API = 6
}

public enum ExportStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
