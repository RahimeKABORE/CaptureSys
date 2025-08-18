using CaptureSys.Shared.Entities;

namespace CaptureSys.ScriptExecutionService.Domain.Entities;

public class ScriptJob : BaseEntity
{
    public string ScriptName { get; private set; } = string.Empty;
    public ScriptType ScriptType { get; private set; }
    public string ScriptPath { get; private set; } = string.Empty;
    public ExecutionStatus Status { get; private set; }
    public Dictionary<string, object> Parameters { get; private set; } = new();
    public string? Output { get; private set; }
    public string? ErrorOutput { get; private set; }
    public int? ExitCode { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public double? ExecutionTimeMs { get; private set; }

    private ScriptJob() { } // EF Constructor

    public ScriptJob(string scriptName, ScriptType scriptType, string scriptPath)
    {
        ScriptName = scriptName;
        ScriptType = scriptType;
        ScriptPath = scriptPath;
        Status = ExecutionStatus.Pending;
        Parameters = new Dictionary<string, object>();
    }

    public void Start()
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string output, int exitCode, double executionTimeMs)
    {
        Status = exitCode == 0 ? ExecutionStatus.Completed : ExecutionStatus.Failed;
        Output = output;
        ExitCode = exitCode;
        ExecutionTimeMs = executionTimeMs;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string errorOutput, int? exitCode = null)
    {
        Status = ExecutionStatus.Failed;
        ErrorOutput = errorOutput;
        ExitCode = exitCode;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = ExecutionStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddParameter(string key, object value)
    {
        Parameters[key] = value;
    }
}

public enum ScriptType
{
    Python = 1,
    PowerShell = 2,
    Bash = 3,
    Batch = 4,
    NodeJs = 5,
    Custom = 6
}

public enum ExecutionStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
