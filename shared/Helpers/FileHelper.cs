namespace CaptureSys.Shared.Helpers;

/// <summary>
/// Utilitaires pour la gestion des fichiers
/// </summary>
public static class FileHelper
{
    private static readonly string[] SupportedImageExtensions = 
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".gif"
    };

    private static readonly string[] SupportedDocumentExtensions = 
    {
        ".pdf", ".doc", ".docx", ".txt", ".rtf"
    };

    /// <summary>
    /// Vérifie si le fichier est une image supportée
    /// </summary>
    public static bool IsImageFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return SupportedImageExtensions.Contains(extension);
    }

    /// <summary>
    /// Vérifie si le fichier est un document supporté
    /// </summary>
    public static bool IsDocumentFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return SupportedDocumentExtensions.Contains(extension);
    }

    /// <summary>
    /// Obtient le type MIME d'un fichier
    /// </summary>
    public static string GetMimeType(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return "application/octet-stream";

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".rtf" => "application/rtf",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Crée un répertoire de manière sécurisée
    /// </summary>
    public static bool EnsureDirectoryExists(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Génère un nom de fichier unique
    /// </summary>
    public static string GenerateUniqueFileName(string originalFileName, string? suffix = null)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        var safeName = ToSafeFileName(nameWithoutExtension);
        var suffixPart = !string.IsNullOrEmpty(suffix) ? $"_{suffix}" : "";
        
        return $"{safeName}_{timestamp}_{uniqueId}{suffixPart}{extension}";
    }

    /// <summary>
    /// Génère un nom de fichier sécurisé
    /// </summary>
    public static string ToSafeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "unnamed";

        var invalidChars = Path.GetInvalidFileNameChars();
        var safeFileName = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        
        return string.IsNullOrWhiteSpace(safeFileName) ? "unnamed" : safeFileName;
    }

    /// <summary>
    /// Calcule la taille de fichier en format lisible
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
}