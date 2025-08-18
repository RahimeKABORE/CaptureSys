using CaptureSys.Shared.Entities;

namespace CaptureSys.ExportService.Domain.Entities;

public class ExportConfiguration : BaseEntity
{
    public string Name { get; private set; }
    public ExportFormat Format { get; private set; }
    public ExportDestination Destination { get; private set; }
    public Dictionary<string, object> Settings { get; private set; }
    public List<string> FieldsToExport { get; private set; }
    public Dictionary<string, string> FieldMappings { get; private set; }
    public bool IncludeMetadata { get; private set; }
    public bool CompressOutput { get; private set; }
    public string? EncryptionKey { get; private set; }

    public ExportConfiguration(
        string name,
        ExportFormat format,
        ExportDestination destination,
        List<string> fieldsToExport,
        Dictionary<string, string>? fieldMappings = null,
        bool includeMetadata = true)
    {
        Name = name;
        Format = format;
        Destination = destination;
        FieldsToExport = fieldsToExport ?? new List<string>();
        FieldMappings = fieldMappings ?? new Dictionary<string, string>();
        IncludeMetadata = includeMetadata;
        Settings = new Dictionary<string, object>();
        CompressOutput = false;
    }

    public void AddSetting(string key, object value)
    {
        Settings[key] = value;
    }

    public T? GetSetting<T>(string key)
    {
        return Settings.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default;
    }

    public void SetCompression(bool compress)
    {
        CompressOutput = compress;
    }

    public void SetEncryption(string? encryptionKey)
    {
        EncryptionKey = encryptionKey;
    }

    public void UpdateFieldMappings(Dictionary<string, string> mappings)
    {
        FieldMappings = mappings ?? new Dictionary<string, string>();
    }
}
