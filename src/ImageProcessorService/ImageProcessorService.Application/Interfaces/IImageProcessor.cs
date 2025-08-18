using CaptureSys.ImageProcessorService.Domain.Entities;

namespace CaptureSys.ImageProcessorService.Application.Interfaces;

public interface IImageProcessor
{
    Task<byte[]> ProcessAsync(byte[] imageData, ImageOperation operation, Dictionary<string, object> parameters);
}
