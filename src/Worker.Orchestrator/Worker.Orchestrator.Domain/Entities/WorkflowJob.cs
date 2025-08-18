using CaptureSys.Shared.Entities;

namespace CaptureSys.Worker.Orchestrator.Domain.Entities;

public class WorkflowJob : BaseEntity
{
    public string DocumentId { get; private set; }
    public string BatchId { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public List<WorkflowStep> Steps { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; }

    public WorkflowJob(string documentId, string batchId)
    {
        DocumentId = documentId;
        BatchId = batchId;
        Status = WorkflowStatus.Pending;
        Steps = new List<WorkflowStep>();
        StartedAt = DateTime.UtcNow;
        Metadata = new Dictionary<string, object>();
        InitializeSteps();
    }

    public void StartProcessing()
    {
        Status = WorkflowStatus.Processing;
    }

    public void CompleteStep(WorkflowStepType stepType, bool success, string? result = null, string? error = null)
    {
        var step = Steps.FirstOrDefault(s => s.Type == stepType);
        if (step != null)
        {
            step.Complete(success, result, error);
            
            if (!success)
            {
                Status = WorkflowStatus.Failed;
                ErrorMessage = error;
            }
            else if (Steps.All(s => s.Status == StepStatus.Completed))
            {
                Status = WorkflowStatus.Completed;
                CompletedAt = DateTime.UtcNow;
            }
        }
    }

    public void Fail(string errorMessage)
    {
        Status = WorkflowStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    private void InitializeSteps()
    {
        Steps.Add(new WorkflowStep(WorkflowStepType.Ingestion, 1));
        Steps.Add(new WorkflowStep(WorkflowStepType.OCR, 2));
        Steps.Add(new WorkflowStep(WorkflowStepType.Classification, 3));
        Steps.Add(new WorkflowStep(WorkflowStepType.Extraction, 4));
        Steps.Add(new WorkflowStep(WorkflowStepType.Export, 5));
    }
}

public class WorkflowStep
{
    public WorkflowStepType Type { get; private set; }
    public int Order { get; private set; }
    public StepStatus Status { get; private set; }
    public string? Result { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public WorkflowStep(WorkflowStepType type, int order)
    {
        Type = type;
        Order = order;
        Status = StepStatus.Pending;
    }

    public void Start()
    {
        Status = StepStatus.Processing;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(bool success, string? result = null, string? error = null)
    {
        Status = success ? StepStatus.Completed : StepStatus.Failed;
        Result = result;
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
    }
}

public enum WorkflowStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum WorkflowStepType
{
    Ingestion = 1,
    OCR = 2,
    Classification = 3,
    Extraction = 4,
    Export = 5
}

public enum StepStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Skipped = 5
}
