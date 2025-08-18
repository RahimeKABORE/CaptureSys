using CaptureSys.ExportService.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.ExportService.Application.Interfaces;

public interface IExportProcessor
{
    Task<Result<ExportJob>> CreateExportJobAsync(string name, ExportFormat format, ExportDestination destination, 
        List<Guid> documentIds, ExportConfiguration configuration, string requestedBy);
    Task<Result<string>> ProcessExportJobAsync(Guid jobId);
    Task<Result<ExportJob>> GetExportJobAsync(Guid jobId);
    Task<Result<List<ExportJob>>> GetExportJobsAsync(string? requestedBy = null);
    Task<Result<bool>> CancelExportJobAsync(Guid jobId);
    Task<Result<List<ExportConfiguration>>> GetExportConfigurationsAsync();
    Task<Result<ExportConfiguration>> CreateExportConfigurationAsync(ExportConfiguration configuration);
}
