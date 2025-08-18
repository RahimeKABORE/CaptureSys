using CaptureSys.Shared.Results;
using CaptureSys.ImageProcessorService.Domain.Entities;

namespace CaptureSys.ImageProcessorService.Application.Interfaces;

public interface IImageProcessingService
{
    Task<Result<ImageProcessingJob>> ProcessImageAsync(
        Stream imageStream, 
        string fileName, 
        ImageOperation operation, 
        Dictionary<string, object>? parameters = null);

    Task<Result<ImageProcessingJob>> GetJobStatusAsync(Guid jobId);
    Task<Result<List<ImageProcessingJob>>> GetActiveJobsAsync();
    Task<Result<byte[]>> GetProcessedImageAsync(Guid jobId);
    Task<Result<bool>> CancelJobAsync(Guid jobId);
}
