using Microsoft.Extensions.Logging;

namespace CaptureSys.AutoLearningService.Infrastructure.Services;

public class ModelTrainer : IModelTrainer
{
    private readonly ILogger<ModelTrainer> _logger;

    public ModelTrainer(ILogger<ModelTrainer> logger)
    {
        _logger = logger;
    }

    // TODO: Implémenter les méthodes de ModelTrainer
}

public interface IModelTrainer
{
    // TODO: Définir l'interface
}
