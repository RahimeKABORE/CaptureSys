namespace CaptureSys.IngestionService.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string? folder = null);
    Task<bool> DeleteFileAsync(string filePath);
    Task<Stream?> GetFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
}
