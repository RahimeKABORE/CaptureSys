using Microsoft.Extensions.DependencyInjection;

namespace CaptureSys.IngestionService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR pour CQRS
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        // AutoMapper pour les mappings
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        
        return services;
    }
}
