using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CaptureSys.ClassificationService.Application.Interfaces;
using CaptureSys.ClassificationService.Infrastructure.Services;

namespace CaptureSys.ClassificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Classification services
        services.AddSingleton<IDocumentClassifier, MLNetDocumentClassifier>();
        return services;
    }
}