using Microsoft.Extensions.Logging;

namespace CaptureSys.AutoLearningService.Infrastructure.Services;

public class ModelStorage : IModelStorage
{
    private readonly ILogger<ModelStorage> _logger;

    public ModelStorage(ILogger<ModelStorage> logger)
    {
        _logger = logger;
    }

    // TODO: Implémenter les méthodes de ModelStorage
}

public interface IModelStorage
{
    // TODO: Définir l'interface
}
