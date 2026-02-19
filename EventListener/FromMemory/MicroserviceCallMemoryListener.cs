using Application.Ports;
using Domain.Containers.MemoryEvent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventListener.FromMemory;

internal class MicroserviceCallMemoryListener : BackgroundService
{
    private readonly MicroserviceCallMemoryQueue _container;
    private readonly ILogger<MicroserviceCallMemoryListener> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MicroserviceCallMemoryListener(
        MicroserviceCallMemoryQueue container,
        ILogger<MicroserviceCallMemoryListener> logger,
        IServiceScopeFactory scopeFactory)
    {
        _container = container;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var myEvent in _container.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var tracePort = scope.ServiceProvider.GetRequiredService<IMicroserviceCallTracePort>();
                var sendResult = await tracePort.SendAsync(myEvent, stoppingToken);
                if (!sendResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "SQS send failed TraceId={TraceId} Error={Error}",
                        myEvent.TraceId,
                        sendResult.ErrorMessage);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQS send failed TraceId={TraceId}", myEvent.TraceId);
            }
        }
    }
}
