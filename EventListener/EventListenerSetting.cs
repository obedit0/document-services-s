using EventListener.FromMemory;
using Microsoft.Extensions.DependencyInjection;

namespace EventListener;

public static class EventListenerSetting
{
    public static void AddInMemoryEventListenerServices(this IServiceCollection services)
    {
        // Hosted Services
        services.AddHostedService<MicroserviceCallMemoryListener>();
        services.AddHostedService<MicroserviceErrorMemoryListener>();
    }
}
