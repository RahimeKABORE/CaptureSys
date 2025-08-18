using ImageProcessorService.Domain;

namespace ImageProcessorService.Application;

public interface IImageProcessingService
{
    Task<byte[]> ProcessAsync(byte[] imageData, ImageOperation operation, IDictionary<string, object>? parameters = null);
}
