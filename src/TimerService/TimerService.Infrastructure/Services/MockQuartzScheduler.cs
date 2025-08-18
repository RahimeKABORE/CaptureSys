using Microsoft.Extensions.Logging;
using CaptureSys.Shared.Results;
using CaptureSys.TimerService.Application.Services;
using CaptureSys.TimerService.Domain.Entities;

namespace CaptureSys.TimerService.Infrastructure.Services;

public class MockQuartzScheduler : IQuartzScheduler
{
    private readonly ILogger<MockQuartzScheduler> _logger;

    public MockQuartzScheduler(ILogger<MockQuartzScheduler> logger)
    {
        _logger = logger;
    }

    public Task<Result<bool>> ScheduleCronJobAsync(ScheduledJob job)
    {
        _logger.LogInformation("Mock: Job CRON {JobId} planifié", job.Id);
        return Task.FromResult(Result<bool>.Success(true));
    }

    public Task<Result<bool>> ScheduleSimpleJobAsync(ScheduledJob job)
    {
        _logger.LogInformation("Mock: Job simple {JobId} planifié", job.Id);
        return Task.FromResult(Result<bool>.Success(true));
    }

    public Task<Result<bool>> ScheduleOnceJobAsync(ScheduledJob job, DateTime executeAt)
    {
        _logger.LogInformation("Mock: Job unique {JobId} planifié pour {ExecuteAt}", job.Id, executeAt);
        return Task.FromResult(Result<bool>.Success(true));
    }

    public Task<Result<bool>> PauseJobAsync(ScheduledJob job)
    {
        _logger.LogInformation("Mock: Job {JobId} mis en pause", job.Id);
        return Task.FromResult(Result<bool>.Success(true));
    }

    public Task<Result<bool>> ResumeJobAsync(ScheduledJob job)
    {
        _logger.LogInformation("Mock: Job {JobId} repris", job.Id);
        return Task.FromResult(Result<bool>.Success(true));
    }

    public Task<Result<bool>> DeleteJobAsync(ScheduledJob job)
    {
        _logger.LogInformation("Mock: Job {JobId} supprimé", job.Id);
        return Task.FromResult(Result<bool>.Success(true));
    }

    public Task<Result<bool>> TriggerJobAsync(ScheduledJob job)
    {
        _logger.LogInformation("Mock: Job {JobId} déclenché", job.Id);
        return Task.FromResult(Result<bool>.Success(true));
    }
}
