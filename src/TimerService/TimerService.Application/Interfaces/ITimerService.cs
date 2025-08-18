using CaptureSys.Shared.Results;
using CaptureSys.TimerService.Domain.Entities;

namespace CaptureSys.TimerService.Application.Interfaces;

public interface ITimerService
{
    Task<Result<ScheduledJob>> CreateCronJobAsync(string jobName, string jobGroup, string cronExpression, string targetService, string targetEndpoint, Dictionary<string, object>? jobData = null);
    Task<Result<ScheduledJob>> CreateSimpleJobAsync(string jobName, string jobGroup, string interval, string targetService, string targetEndpoint, int? maxExecutions = null, Dictionary<string, object>? jobData = null);
    Task<Result<ScheduledJob>> CreateOnceJobAsync(string jobName, string jobGroup, DateTime executeAt, string targetService, string targetEndpoint, Dictionary<string, object>? jobData = null);
    Task<Result<ScheduledJob>> GetJobAsync(Guid jobId);
    Task<Result<List<ScheduledJob>>> GetAllJobsAsync();
    Task<Result<List<ScheduledJob>>> GetJobsByStatusAsync(JobStatus status);
    Task<Result<bool>> StartJobAsync(Guid jobId);
    Task<Result<bool>> PauseJobAsync(Guid jobId);
    Task<Result<bool>> StopJobAsync(Guid jobId);
    Task<Result<bool>> DeleteJobAsync(Guid jobId);
    Task<Result<bool>> TriggerJobNowAsync(Guid jobId);
}
