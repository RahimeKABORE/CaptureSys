using CaptureSys.Shared.Entities;

namespace CaptureSys.TimerService.Domain.Entities;

public class ScheduledJob : BaseEntity
{
    public string JobName { get; private set; } = string.Empty;
    public string JobGroup { get; private set; } = "DEFAULT";
    public TriggerType TriggerType { get; private set; }
    public string CronExpression { get; private set; } = string.Empty;
    public string? SimpleInterval { get; private set; }
    public JobStatus Status { get; private set; }
    public string TargetService { get; private set; } = string.Empty;
    public string TargetEndpoint { get; private set; } = string.Empty;
    public string HttpMethod { get; private set; } = "POST";
    public Dictionary<string, object> JobData { get; private set; } = new();
    public DateTime? NextFireTime { get; private set; }
    public DateTime? LastFireTime { get; private set; }
    public int ExecutionCount { get; private set; }
    public int? MaxExecutions { get; private set; }
    public string? LastExecutionResult { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    private ScheduledJob() { } // EF Constructor

    public ScheduledJob(string jobName, string jobGroup, TriggerType triggerType, string targetService, string targetEndpoint)
    {
        JobName = jobName;
        JobGroup = jobGroup;
        TriggerType = triggerType;
        TargetService = targetService;
        TargetEndpoint = targetEndpoint;
        Status = JobStatus.Scheduled;
        JobData = new Dictionary<string, object>();
    }

    public void SetCronTrigger(string cronExpression, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (TriggerType != TriggerType.Cron)
            throw new InvalidOperationException("Job must be of type Cron to set cron expression");

        CronExpression = cronExpression;
        StartDate = startDate ?? DateTime.UtcNow;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSimpleTrigger(string interval, int? maxExecutions = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (TriggerType != TriggerType.Simple)
            throw new InvalidOperationException("Job must be of type Simple to set interval");

        SimpleInterval = interval;
        MaxExecutions = maxExecutions;
        StartDate = startDate ?? DateTime.UtcNow;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = JobStatus.Running;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        Status = JobStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Stop()
    {
        Status = JobStatus.Stopped;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateExecution(DateTime fireTime, string? result = null)
    {
        LastFireTime = fireTime;
        ExecutionCount++;
        LastExecutionResult = result;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNextFireTime(DateTime nextFireTime)
    {
        NextFireTime = nextFireTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddJobData(string key, object value)
    {
        JobData[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum TriggerType
{
    Cron = 1,
    Simple = 2,
    Once = 3
}

public enum JobStatus
{
    Scheduled = 1,
    Running = 2,
    Paused = 3,
    Stopped = 4,
    Completed = 5,
    Error = 6
}
