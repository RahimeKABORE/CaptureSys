using Microsoft.Extensions.Logging;
using CaptureSys.Shared.Results;
using CaptureSys.ScriptExecutionService.Application.Interfaces;
using CaptureSys.ScriptExecutionService.Domain.Entities;

namespace CaptureSys.ScriptExecutionService.Application.Services;

public class ScriptExecutionApplicationService : IScriptExecutionService
{
    private readonly ILogger<ScriptExecutionApplicationService> _logger;
    private readonly IScriptRunner _scriptRunner;
    private readonly Dictionary<Guid, ScriptJob> _activeJobs;

    public ScriptExecutionApplicationService(
        ILogger<ScriptExecutionApplicationService> logger,
        IScriptRunner scriptRunner)
    {
        _logger = logger;
        _scriptRunner = scriptRunner;
        _activeJobs = new Dictionary<Guid, ScriptJob>();
    }

    public Task<Result<ScriptJob>> ExecuteScriptAsync(string scriptName, ScriptType scriptType, string scriptContent, Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Démarrage de l'exécution du script: {ScriptName}, Type: {ScriptType}", scriptName, scriptType);

            // Créer un fichier temporaire pour le script
            var tempPath = Path.GetTempFileName();
            var extension = GetScriptExtension(scriptType);
            var scriptPath = Path.ChangeExtension(tempPath, extension);
            File.WriteAllText(scriptPath, scriptContent);

            var job = new ScriptJob(scriptName, scriptType, scriptPath);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    job.AddParameter(param.Key, param.Value);
                }
            }

            _activeJobs[job.Id] = job;
            _ = Task.Run(async () => await ExecuteScriptBackgroundAsync(job));

            return Task.FromResult(Result<ScriptJob>.Success(job));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du démarrage de l'exécution du script");
            return Task.FromResult(Result<ScriptJob>.Failure($"Erreur: {ex.Message}"));
        }
    }

    public Task<Result<ScriptJob>> GetJobStatusAsync(Guid jobId)
    {
        if (_activeJobs.TryGetValue(jobId, out var job))
        {
            return Task.FromResult(Result<ScriptJob>.Success(job));
        }

        return Task.FromResult(Result<ScriptJob>.Failure("Job non trouvé"));
    }

    public Task<Result<List<ScriptJob>>> GetActiveJobsAsync()
    {
        var activeJobs = _activeJobs.Values.Where(j => j.Status == ExecutionStatus.Running).ToList();
        return Task.FromResult(Result<List<ScriptJob>>.Success(activeJobs));
    }

    public Task<Result<bool>> CancelJobAsync(Guid jobId)
    {
        if (_activeJobs.TryGetValue(jobId, out var job) && job.Status == ExecutionStatus.Running)
        {
            job.Cancel();
            _logger.LogInformation("Script {JobId} annulé", jobId);
            return Task.FromResult(Result<bool>.Success(true));
        }

        return Task.FromResult(Result<bool>.Failure("Job non trouvé ou non annulable"));
    }

    public Task<Result<string>> GetJobOutputAsync(Guid jobId)
    {
        if (_activeJobs.TryGetValue(jobId, out var job) && !string.IsNullOrEmpty(job.Output))
        {
            return Task.FromResult(Result<string>.Success(job.Output));
        }

        return Task.FromResult(Result<string>.Failure("Sortie non trouvée ou job non terminé"));
    }

    public Task<Result<List<ScriptType>>> GetSupportedScriptTypesAsync()
    {
        var supportedTypes = Enum.GetValues<ScriptType>().ToList();
        return Task.FromResult(Result<List<ScriptType>>.Success(supportedTypes));
    }

    private async Task ExecuteScriptBackgroundAsync(ScriptJob job)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Exécution du script {JobId} commencée", job.Id);
            job.Start();

            var result = await _scriptRunner.RunScriptAsync(job.ScriptPath, job.ScriptType, job.Parameters);
            
            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            job.Complete(result.Output, result.ExitCode, executionTime);

            _logger.LogInformation("Exécution du script {JobId} terminée avec le code {ExitCode} en {ExecutionTime}ms", 
                job.Id, result.ExitCode, executionTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'exécution du script {JobId}", job.Id);
            job.Fail($"Erreur d'exécution: {ex.Message}");
        }
        finally
        {
            // Nettoyer le fichier temporaire
            try
            {
                if (File.Exists(job.ScriptPath))
                {
                    File.Delete(job.ScriptPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible de supprimer le fichier temporaire {ScriptPath}", job.ScriptPath);
            }
        }
    }

    private static string GetScriptExtension(ScriptType scriptType)
    {
        return scriptType switch
        {
            ScriptType.Python => ".py",
            ScriptType.PowerShell => ".ps1",
            ScriptType.Bash => ".sh",
            ScriptType.Batch => ".bat",
            ScriptType.NodeJs => ".js",
            ScriptType.Custom => ".script",
            _ => ".txt"
        };
    }
}

public interface IScriptRunner
{
    Task<ScriptResult> RunScriptAsync(string scriptPath, ScriptType scriptType, Dictionary<string, object> parameters);
}

public class ScriptResult
{
    public string Output { get; set; } = string.Empty;
    public int ExitCode { get; set; }
}
