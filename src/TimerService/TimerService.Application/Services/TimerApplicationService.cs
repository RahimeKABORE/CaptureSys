using Microsoft.Extensions.Logging;
using CaptureSys.Shared.Results;
using CaptureSys.TimerService.Application.Interfaces;
using CaptureSys.TimerService.Domain.Entities;

namespace CaptureSys.TimerService.Application.Services;

public class TimerApplicationService : ITimerService
{
    private readonly ILogger<TimerApplicationService> _logger;
    private readonly IQuartzScheduler _quartzScheduler;
    private readonly Dictionary<Guid, ScheduledJob> _jobs;

    public TimerApplicationService(
        ILogger<TimerApplicationService> logger,
        IQuartzScheduler quartzScheduler)
    {
        _logger = logger;
        _quartzScheduler = quartzScheduler;
        _jobs = new Dictionary<Guid, ScheduledJob>();
    }

    public async Task<Result<ScheduledJob>> CreateCronJobAsync(string jobName, string jobGroup, string cronExpression, string targetService, string targetEndpoint, Dictionary<string, object>? jobData = null)
    {
        try
        {
            _logger.LogInformation("Création d'un job CRON: {JobName} avec l'expression {CronExpression}", jobName, cronExpression);

            var job = new ScheduledJob(jobName, jobGroup, TriggerType.Cron, targetService, targetEndpoint);
            job.SetCronTrigger(cronExpression);
            
            if (jobData != null)
            {
                foreach (var data in jobData)
                {
                    job.AddJobData(data.Key, data.Value);
                }
            }

            _jobs[job.Id] = job;
            
            var quartzResult = await _quartzScheduler.ScheduleCronJobAsync(job);
            if (quartzResult.IsSuccess)
            {
                job.Activate();
                return Result<ScheduledJob>.Success(job);
            }

            return Result<ScheduledJob>.Failure(quartzResult.Error!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du job CRON {JobName}", jobName);
            return Result<ScheduledJob>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<ScheduledJob>> CreateSimpleJobAsync(string jobName, string jobGroup, string interval, string targetService, string targetEndpoint, int? maxExecutions = null, Dictionary<string, object>? jobData = null)
    {
        try
        {
            _logger.LogInformation("Création d'un job simple: {JobName} avec l'interval {Interval}", jobName, interval);

            var job = new ScheduledJob(jobName, jobGroup, TriggerType.Simple, targetService, targetEndpoint);
            job.SetSimpleTrigger(interval, maxExecutions);
            
            if (jobData != null)
            {
                foreach (var data in jobData)
                {
                    job.AddJobData(data.Key, data.Value);
                }
            }

            _jobs[job.Id] = job;
            
            var quartzResult = await _quartzScheduler.ScheduleSimpleJobAsync(job);
            if (quartzResult.IsSuccess)
            {
                job.Activate();
                return Result<ScheduledJob>.Success(job);
            }

            return Result<ScheduledJob>.Failure(quartzResult.Error!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du job simple {JobName}", jobName);
            return Result<ScheduledJob>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<ScheduledJob>> CreateOnceJobAsync(string jobName, string jobGroup, DateTime executeAt, string targetService, string targetEndpoint, Dictionary<string, object>? jobData = null)
    {
        try
        {
            _logger.LogInformation("Création d'un job unique: {JobName} à exécuter le {ExecuteAt}", jobName, executeAt);

            var job = new ScheduledJob(jobName, jobGroup, TriggerType.Once, targetService, targetEndpoint);
            
            if (jobData != null)
            {
                foreach (var data in jobData)
                {
                    job.AddJobData(data.Key, data.Value);
                }
            }

            _jobs[job.Id] = job;
            
            var quartzResult = await _quartzScheduler.ScheduleOnceJobAsync(job, executeAt);
            if (quartzResult.IsSuccess)
            {
                job.Activate();
                job.SetNextFireTime(executeAt);
                return Result<ScheduledJob>.Success(job);
            }

            return Result<ScheduledJob>.Failure(quartzResult.Error!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du job unique {JobName}", jobName);
            return Result<ScheduledJob>.Failure($"Erreur: {ex.Message}");
        }
    }

    public Task<Result<ScheduledJob>> GetJobAsync(Guid jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            return Task.FromResult(Result<ScheduledJob>.Success(job));
        }

        return Task.FromResult(Result<ScheduledJob>.Failure("Job non trouvé"));
    }

    public Task<Result<List<ScheduledJob>>> GetAllJobsAsync()
    {
        var allJobs = _jobs.Values.ToList();
        return Task.FromResult(Result<List<ScheduledJob>>.Success(allJobs));
    }

    public Task<Result<List<ScheduledJob>>> GetJobsByStatusAsync(JobStatus status)
    {
        var filteredJobs = _jobs.Values.Where(j => j.Status == status).ToList();
        return Task.FromResult(Result<List<ScheduledJob>>.Success(filteredJobs));
    }

    public async Task<Result<bool>> StartJobAsync(Guid jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            var result = await _quartzScheduler.ResumeJobAsync(job);
            if (result.IsSuccess)
            {
                job.Activate();
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure(result.Error!);
        }

        return Result<bool>.Failure("Job non trouvé");
    }

    public async Task<Result<bool>> PauseJobAsync(Guid jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            var result = await _quartzScheduler.PauseJobAsync(job);
            if (result.IsSuccess)
            {
                job.Pause();
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure(result.Error!);
        }

        return Result<bool>.Failure("Job non trouvé");
    }

    public async Task<Result<bool>> StopJobAsync(Guid jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            var result = await _quartzScheduler.DeleteJobAsync(job);
            if (result.IsSuccess)
            {
                job.Stop();
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure(result.Error!);
        }

        return Result<bool>.Failure("Job non trouvé");
    }

    public async Task<Result<bool>> DeleteJobAsync(Guid jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            var result = await _quartzScheduler.DeleteJobAsync(job);
            if (result.IsSuccess)
            {
                _jobs.Remove(jobId);
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure(result.Error!);
        }

        return Result<bool>.Failure("Job non trouvé");
    }

    public async Task<Result<bool>> TriggerJobNowAsync(Guid jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            var result = await _quartzScheduler.TriggerJobAsync(job);
            return result;
        }

        return Result<bool>.Failure("Job non trouvé");
    }
}

public interface IQuartzScheduler
{
    Task<Result<bool>> ScheduleCronJobAsync(ScheduledJob job);
    Task<Result<bool>> ScheduleSimpleJobAsync(ScheduledJob job);
    Task<Result<bool>> ScheduleOnceJobAsync(ScheduledJob job, DateTime executeAt);
    Task<Result<bool>> PauseJobAsync(ScheduledJob job);
    Task<Result<bool>> ResumeJobAsync(ScheduledJob job);
    Task<Result<bool>> DeleteJobAsync(ScheduledJob job);
    Task<Result<bool>> TriggerJobAsync(ScheduledJob job);
}
