namespace CaptureSys.ImageProcessorService.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder);
    Task<byte[]> ReadFileAsync(string filePath);
    Task<string> SaveProcessedImageAsync(byte[] imageData, string originalFileName, string operation);
}
