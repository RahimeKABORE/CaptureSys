using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CaptureSys.Gateway.Auth.Application.Interfaces;
using CaptureSys.Gateway.Auth.Infrastructure.Services;

namespace CaptureSys.Gateway.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Auth services
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}