using Microsoft.Extensions.Logging;
using CaptureSys.ImageProcessorService.Application.Interfaces;

namespace CaptureSys.ImageProcessorService.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _basePath;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        var folderPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(folderPath);
        
        var filePath = Path.Combine(folderPath, $"{Guid.NewGuid()}_{fileName}");
        
        using var fileStreamOutput = File.Create(filePath);
        await fileStream.CopyToAsync(fileStreamOutput);
        
        _logger.LogDebug("Fichier sauvegardé: {FilePath}", filePath);
        return filePath;
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Fichier non trouvé: {filePath}");
            
        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task<string> SaveProcessedImageAsync(byte[] imageData, string originalFileName, string operation)
    {
        var outputFolder = Path.Combine(_basePath, "output");
        Directory.CreateDirectory(outputFolder);
        
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);
        var outputFileName = $"{fileNameWithoutExt}_{operation}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{extension}";
        var outputPath = Path.Combine(outputFolder, outputFileName);
        
        await File.WriteAllBytesAsync(outputPath, imageData);
        
        _logger.LogDebug("Image traitée sauvegardée: {OutputPath}", outputPath);
        return outputPath;
    }
}
