using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using CaptureSys.IngestionService.Infrastructure.Data;
using CaptureSys.IngestionService.Infrastructure.Services;
using CaptureSys.IngestionService.Application.Interfaces;

namespace CaptureSys.IngestionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Entity Framework avec SQLite pour les tests
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<IngestionDbContext>(options =>
            options.UseSqlite("Data Source=capturesys_ingestion.db"));
        
        // Services de stockage des fichiers
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        
        // Services de messaging (RabbitMQ)
        services.AddScoped<IEventPublisher, RabbitMQEventPublisher>();
        
        return services;
    }
}
