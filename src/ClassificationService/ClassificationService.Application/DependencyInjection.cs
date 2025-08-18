using Microsoft.Extensions.DependencyInjection;

namespace CaptureSys.ClassificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application services would be added here
        // For now, just return services as this layer contains mainly interfaces
        
        return services;
    }
}
