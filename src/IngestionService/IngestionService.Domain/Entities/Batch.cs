using CaptureSys.Shared.Entities;
using CaptureSys.Shared.DTOs;

namespace CaptureSys.IngestionService.Domain.Entities;

public class Batch : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public BatchStatus Status { get; private set; }
    public string? ProjectName { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Dictionary<string, object> Settings { get; private set; } = new();

    private readonly List<Document> _documents = new();
    public IReadOnlyList<Document> Documents => _documents.AsReadOnly();

    private Batch() { } // EF Constructor

    public Batch(string name, string? description = null, string? projectName = null)
    {
        Name = name;
        Description = description;
        ProjectName = projectName;
        Status = BatchStatus.Created;
    }

    public void AddDocument(Document document)
    {
        _documents.Add(document);
        MarkAsUpdated();
    }

    public void StartProcessing()
    {
        Status = BatchStatus.Processing;
        StartedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Complete()
    {
        Status = BatchStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
}
