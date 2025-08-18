using CaptureSys.Worker.Orchestrator.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.Worker.Orchestrator.Application.Interfaces;

public interface IWorkflowOrchestrator
{
    Task<Result<WorkflowJob>> StartWorkflowAsync(string documentId, string batchId);
    Task<Result<WorkflowJob>> GetWorkflowStatusAsync(Guid jobId);
    Task<Result<List<WorkflowJob>>> GetActiveWorkflowsAsync();
    Task<Result<bool>> CancelWorkflowAsync(Guid jobId);
    Task ProcessPendingWorkflowsAsync();
}
