using Microsoft.Extensions.Logging;
using Quartz;
using CaptureSys.Shared.Results;
using CaptureSys.TimerService.Application.Interfaces;
using CaptureSys.TimerService.Application.Services;
using CaptureSys.TimerService.Domain.Entities;

namespace CaptureSys.TimerService.Infrastructure.Services;

public class QuartzScheduler : IQuartzScheduler
{
    private readonly ILogger<QuartzScheduler> _logger;
    private readonly IScheduler _scheduler;

    public QuartzScheduler(ILogger<QuartzScheduler> logger, IScheduler scheduler)
    {
        _logger = logger;
        _scheduler = scheduler;
    }

    public async Task<Result<bool>> ScheduleCronJobAsync(ScheduledJob job)
    {
        try
        {
            var quartzJob = JobBuilder.Create<HttpJobExecutor>()
                .WithIdentity(job.Id.ToString(), job.JobGroup)
                .UsingJobData("jobId", job.Id.ToString())
                .UsingJobData("targetService", job.TargetService)
                .UsingJobData("targetEndpoint", job.TargetEndpoint)
                .UsingJobData("httpMethod", job.HttpMethod)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{job.Id}_trigger", job.JobGroup)
                .WithCronSchedule(job.CronExpression)
                .Build();

            await _scheduler.ScheduleJob(quartzJob, trigger);
            
            _logger.LogInformation("Job CRON {JobId} planifié avec succès", job.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la planification du job CRON {JobId}", job.Id);
            return Result<bool>.Failure($"Erreur Quartz: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ScheduleSimpleJobAsync(ScheduledJob job)
    {
        try
        {
            var quartzJob = JobBuilder.Create<HttpJobExecutor>()
                .WithIdentity(job.Id.ToString(), job.JobGroup)
                .UsingJobData("jobId", job.Id.ToString())
                .UsingJobData("targetService", job.TargetService)
                .UsingJobData("targetEndpoint", job.TargetEndpoint)
                .UsingJobData("httpMethod", job.HttpMethod)
                .Build();

            // Parse interval (ex: "30s", "5m", "1h")
            var interval = ParseInterval(job.SimpleInterval!);
            
            var triggerBuilder = TriggerBuilder.Create()
                .WithIdentity($"{job.Id}_trigger", job.JobGroup)
                .WithSimpleSchedule(x =>
                {
                    x.WithInterval(interval);
                    if (job.MaxExecutions.HasValue)
                        x.WithRepeatCount(job.MaxExecutions.Value - 1);
                    else
                        x.RepeatForever();
                });

            var trigger = triggerBuilder.Build();
            await _scheduler.ScheduleJob(quartzJob, trigger);
            
            _logger.LogInformation("Job simple {JobId} planifié avec succès", job.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la planification du job simple {JobId}", job.Id);
            return Result<bool>.Failure($"Erreur Quartz: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ScheduleOnceJobAsync(ScheduledJob job, DateTime executeAt)
    {
        try
        {
            var quartzJob = JobBuilder.Create<HttpJobExecutor>()
                .WithIdentity(job.Id.ToString(), job.JobGroup)
                .UsingJobData("jobId", job.Id.ToString())
                .UsingJobData("targetService", job.TargetService)
                .UsingJobData("targetEndpoint", job.TargetEndpoint)
                .UsingJobData("httpMethod", job.HttpMethod)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{job.Id}_trigger", job.JobGroup)
                .StartAt(executeAt)
                .Build();

            await _scheduler.ScheduleJob(quartzJob, trigger);
            
            _logger.LogInformation("Job unique {JobId} planifié pour {ExecuteAt}", job.Id, executeAt);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la planification du job unique {JobId}", job.Id);
            return Result<bool>.Failure($"Erreur Quartz: {ex.Message}");
        }
    }

    public async Task<Result<bool>> PauseJobAsync(ScheduledJob job)
    {
        try
        {
            await _scheduler.PauseJob(new JobKey(job.Id.ToString(), job.JobGroup));
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ResumeJobAsync(ScheduledJob job)
    {
        try
        {
            await _scheduler.ResumeJob(new JobKey(job.Id.ToString(), job.JobGroup));
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteJobAsync(ScheduledJob job)
    {
        try
        {
            await _scheduler.DeleteJob(new JobKey(job.Id.ToString(), job.JobGroup));
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<bool>> TriggerJobAsync(ScheduledJob job)
    {
        try
        {
            await _scheduler.TriggerJob(new JobKey(job.Id.ToString(), job.JobGroup));
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Erreur: {ex.Message}");
        }
    }

    private static TimeSpan ParseInterval(string interval)
    {
        var value = int.Parse(interval[..^1]);
        var unit = interval[^1];

        return unit switch
        {
            's' => TimeSpan.FromSeconds(value),
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            'd' => TimeSpan.FromDays(value),
            _ => throw new ArgumentException($"Unité d'intervalle non supportée: {unit}")
        };
    }
}

[DisallowConcurrentExecution]
public class HttpJobExecutor : IJob
{
    private readonly ILogger<HttpJobExecutor> _logger;
    private readonly HttpClient _httpClient;

    public HttpJobExecutor(ILogger<HttpJobExecutor> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobData = context.JobDetail.JobDataMap;
        var jobId = jobData.GetString("jobId");
        var targetService = jobData.GetString("targetService");
        var targetEndpoint = jobData.GetString("targetEndpoint");
        var httpMethod = jobData.GetString("httpMethod") ?? "POST";

        try
        {
            _logger.LogInformation("Exécution du job {JobId} vers {TargetService}{TargetEndpoint}", 
                jobId, targetService, targetEndpoint);

            var url = $"http://{targetService}{targetEndpoint}";
            HttpResponseMessage response;

            if (httpMethod.ToUpper() == "GET")
            {
                response = await _httpClient.GetAsync(url);
            }
            else
            {
                response = await _httpClient.PostAsync(url, null);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Job {JobId} exécuté avec succès. Status: {StatusCode}", 
                    jobId, response.StatusCode);
            }
            else
            {
                _logger.LogWarning("Job {JobId} terminé avec erreur. Status: {StatusCode}", 
                    jobId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'exécution du job {JobId}", jobId);
            throw;
        }
    }
}
