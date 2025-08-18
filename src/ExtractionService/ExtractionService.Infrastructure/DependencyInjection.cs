using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CaptureSys.ExtractionService.Application.Interfaces;
using CaptureSys.ExtractionService.Infrastructure.Services;

namespace CaptureSys.ExtractionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Extraction services
        services.AddScoped<IExtractionEngine, RegexExtractionEngine>();
        services.AddScoped<IFieldExtractor, FieldExtractorService>();

        return services;
    }
}
