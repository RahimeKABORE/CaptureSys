using ImageProcessorService.Application;
using ImageProcessorService.Domain;

namespace ImageProcessorService.Infrastructure;

public class ImageProcessingService : IImageProcessingService
{
    public async Task<byte[]> ProcessAsync(byte[] imageData, ImageOperation operation, IDictionary<string, object>? parameters = null)
    {
        // TODO: Implémenter le traitement réel selon l'opération demandée
        await Task.Delay(50); // Simulation
        return imageData;
    }
}
