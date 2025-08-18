using Microsoft.Extensions.Logging;
using CaptureSys.Worker.Orchestrator.Application.Interfaces;
using CaptureSys.Worker.Orchestrator.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.Worker.Orchestrator.Infrastructure.Services;

public class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private readonly ILogger<WorkflowOrchestrator> _logger;
    private readonly IServiceCommunicator _serviceCommunicator;
    private readonly Dictionary<Guid, WorkflowJob> _activeJobs;

    public WorkflowOrchestrator(ILogger<WorkflowOrchestrator> logger, IServiceCommunicator serviceCommunicator)
    {
        _logger = logger;
        _serviceCommunicator = serviceCommunicator;
        _activeJobs = new Dictionary<Guid, WorkflowJob>();
    }

    public async Task<Result<WorkflowJob>> StartWorkflowAsync(string documentId, string batchId)
    {
        try
        {
            await Task.Yield();
            
            var job = new WorkflowJob(documentId, batchId);
            _activeJobs[job.Id] = job;
            
            job.StartProcessing();
            _logger.LogInformation("Workflow démarré pour Document {DocumentId}, Batch {BatchId}, Job {JobId}", 
                documentId, batchId, job.Id);

            // Démarrer le traitement en arrière-plan
            _ = Task.Run(() => ProcessWorkflowAsync(job));

            return Result<WorkflowJob>.Success(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du démarrage du workflow");
            return Result<WorkflowJob>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<WorkflowJob>> GetWorkflowStatusAsync(Guid jobId)
    {
        await Task.Yield();
        
        if (_activeJobs.TryGetValue(jobId, out var job))
        {
            return Result<WorkflowJob>.Success(job);
        }

        return Result<WorkflowJob>.Failure("Workflow non trouvé");
    }

    public async Task<Result<List<WorkflowJob>>> GetActiveWorkflowsAsync()
    {
        await Task.Yield();
        var activeJobs = _activeJobs.Values.Where(j => j.Status == WorkflowStatus.Processing).ToList();
        return Result<List<WorkflowJob>>.Success(activeJobs);
    }

    public async Task<Result<bool>> CancelWorkflowAsync(Guid jobId)
    {
        await Task.Yield();
        
        if (_activeJobs.TryGetValue(jobId, out var job))
        {
            job.Fail("Workflow annulé par l'utilisateur");
            _logger.LogInformation("Workflow {JobId} annulé", jobId);
            return Result<bool>.Success(true);
        }

        return Result<bool>.Failure("Workflow non trouvé");
    }

    public async Task ProcessPendingWorkflowsAsync()
    {
        await Task.Yield();
        
        var pendingJobs = _activeJobs.Values.Where(j => j.Status == WorkflowStatus.Pending).ToList();
        
        foreach (var job in pendingJobs)
        {
            _ = Task.Run(() => ProcessWorkflowAsync(job));
        }
    }

    private async Task ProcessWorkflowAsync(WorkflowJob job)
    {
        try
        {
            _logger.LogInformation("Traitement du workflow {JobId} commencé", job.Id);

            // Simulation de traitement pour chaque étape
            await ProcessStepAsync(job, WorkflowStepType.Ingestion, async () =>
            {
                await Task.Delay(1000); // Simulation
                return Result<string>.Success("Ingestion terminée");
            });

            if (job.Status == WorkflowStatus.Failed) return;

            await ProcessStepAsync(job, WorkflowStepType.OCR, async () =>
            {
                await Task.Delay(2000); // Simulation OCR plus long
                return Result<string>.Success("OCR terminé");
            });

            if (job.Status == WorkflowStatus.Failed) return;

            // Étape 3: Classification
            await ProcessStepAsync(job, WorkflowStepType.Classification, async () =>
            {
                return await _serviceCommunicator.ClassifyDocumentAsync(job.DocumentId);
            });

            if (job.Status == WorkflowStatus.Failed) return;

            // Étape 4: Extraction
            await ProcessStepAsync(job, WorkflowStepType.Extraction, async () =>
            {
                return await _serviceCommunicator.ExtractDataAsync(job.DocumentId);
            });

            if (job.Status == WorkflowStatus.Failed) return;

            // Étape 5: Export
            await ProcessStepAsync(job, WorkflowStepType.Export, async () =>
            {
                return await _serviceCommunicator.ExportDataAsync(job.DocumentId);
            });

            _logger.LogInformation("Workflow {JobId} terminé avec succès", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement du workflow {JobId}", job.Id);
            job.Fail($"Erreur inattendue: {ex.Message}");
        }
    }

    private async Task ProcessStepAsync(WorkflowJob job, WorkflowStepType stepType, Func<Task<Result<string>>> stepAction)
    {
        var step = job.Steps.First(s => s.Type == stepType);
        step.Start();
        
        _logger.LogInformation("Étape {StepType} démarrée pour le workflow {JobId}", stepType, job.Id);

        try
        {
            var result = await stepAction();
            
            if (result.IsSuccess)
            {
                job.CompleteStep(stepType, true, result.Value);
                _logger.LogInformation("Étape {StepType} terminée avec succès pour le workflow {JobId}", stepType, job.Id);
            }
            else
            {
                job.CompleteStep(stepType, false, null, result.Error);
                _logger.LogWarning("Étape {StepType} échouée pour le workflow {JobId}: {Error}", stepType, job.Id, result.Error);
            }
        }
        catch (Exception ex)
        {
            job.CompleteStep(stepType, false, null, ex.Message);
            _logger.LogError(ex, "Erreur lors de l'étape {StepType} pour le workflow {JobId}", stepType, job.Id);
        }
    }
}
