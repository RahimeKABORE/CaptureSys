using CaptureSys.IngestionService.Application.Interfaces;
using CaptureSys.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace CaptureSys.IngestionService.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _baseStoragePath;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "storage", "documents");
        FileHelper.EnsureDirectoryExists(_baseStoragePath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string? folder = null)
    {
        try
        {
            var safeFileName = FileHelper.GenerateUniqueFileName(fileName);
            var folderPath = string.IsNullOrEmpty(folder) 
                ? _baseStoragePath 
                : Path.Combine(_baseStoragePath, folder);
            
            FileHelper.EnsureDirectoryExists(folderPath);
            
            var filePath = Path.Combine(folderPath, safeFileName);
            
            using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamOutput);
            
            _logger.LogInformation("Fichier sauvegardé: {FilePath}", filePath);
            
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde du fichier {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation("Fichier supprimé: {FilePath}", filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du fichier {FilePath}", filePath);
            return false;
        }
    }

    public async Task<Stream?> GetFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                return await Task.FromResult(new FileStream(filePath, FileMode.Open, FileAccess.Read));
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la lecture du fichier {FilePath}", filePath);
            return null;
        }
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }
}
