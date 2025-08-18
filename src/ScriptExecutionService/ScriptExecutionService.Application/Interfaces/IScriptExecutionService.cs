using CaptureSys.Shared.Results;
using CaptureSys.ScriptExecutionService.Domain.Entities;

namespace CaptureSys.ScriptExecutionService.Application.Interfaces;

public interface IScriptExecutionService
{
    Task<Result<ScriptJob>> ExecuteScriptAsync(string scriptName, ScriptType scriptType, string scriptContent, Dictionary<string, object>? parameters = null);
    Task<Result<ScriptJob>> GetJobStatusAsync(Guid jobId);
    Task<Result<List<ScriptJob>>> GetActiveJobsAsync();
    Task<Result<bool>> CancelJobAsync(Guid jobId);
    Task<Result<string>> GetJobOutputAsync(Guid jobId);
    Task<Result<List<ScriptType>>> GetSupportedScriptTypesAsync();
}
