using CaptureSys.Shared.Events;

namespace CaptureSys.IngestionService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T eventData) where T : DocumentProcessingEvent;
}
