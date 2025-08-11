using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CaptureSys.OcrService.Application.Interfaces;
using CaptureSys.OcrService.Infrastructure.Services;

namespace CaptureSys.OcrService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Service OCR avec Tesseract
        services.AddScoped<IOcrProcessor, TesseractOcrProcessor>();
        
        // TODO: Services de messaging pour écouter les événements
        
        return services;
    }
}
