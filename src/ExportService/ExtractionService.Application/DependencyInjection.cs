using Microsoft.Extensions.DependencyInjection;

namespace CaptureSys.ExportService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application services would be added here
        return services;
    }
}
