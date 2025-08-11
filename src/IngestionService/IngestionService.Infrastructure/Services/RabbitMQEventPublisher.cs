using CaptureSys.IngestionService.Application.Interfaces;
using CaptureSys.Shared.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CaptureSys.IngestionService.Infrastructure.Services;

public class RabbitMQEventPublisher : IEventPublisher
{
    private readonly ILogger<RabbitMQEventPublisher> _logger;

    public RabbitMQEventPublisher(ILogger<RabbitMQEventPublisher> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<T>(T eventData) where T : DocumentProcessingEvent
    {
        try
        {
            // TODO: Implémentation RabbitMQ réelle
            var json = JsonSerializer.Serialize(eventData);
            _logger.LogInformation("Événement publié: {EventType} - {EventData}", 
                typeof(T).Name, json);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la publication de l'événement {EventType}", typeof(T).Name);
            throw;
        }
    }
}
