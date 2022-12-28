using LawEnforcementDialer.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LawEnforcementDialer.PinManager;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPinManager(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPinManager, InMemoryPinManager>();
        services.AddSingleton<IPinRepository, InMemoryPinRepository>();
        services.AddHostedService<PinMonitor>();

        services.Configure<PinManagerConfiguration>(s =>
        {
            configuration.GetSection(PinManagerConfiguration.Name).Bind(s);
        });

        return services;
    }
}