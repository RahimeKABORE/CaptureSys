using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CaptureSys.ExportService.Application.Interfaces;
using CaptureSys.ExportService.Infrastructure.Services;
using CaptureSys.ExportService.Infrastructure.Exporters;

namespace CaptureSys.ExportService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Export services
        services.AddScoped<IExportProcessor, ExportProcessorService>();
        services.AddScoped<CsvExporter>();
        services.AddScoped<JsonExporter>();

        return services;
    }
}
