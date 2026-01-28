using Application.Ports;
using Domain.Containers.MemoryEvent;
using Domain.Entities.Internals;

namespace Application.Usecases.ErrorUsecase;

public sealed class ErrorInternalCase : IErrorInternalPort
{
    private readonly MicroserviceErrorMemoryQueue _queue;

    public ErrorInternalCase(MicroserviceErrorMemoryQueue queue)
    {
        _queue = queue;
    }

    public async Task SaveAsync(MicroserviceErrorEntity entity, CancellationToken ct = default)
    {
        var trace = new MicroserviceErrorTraceEntity
        {
            Identity = entity.ErrorCode,
            TraceId = entity.MessageIdentification ?? string.Empty,
            ChannelId = entity.ChannelId.ToString(),
            DeviceId = "unknown",
            RequestUrl = entity.Endpoint ?? string.Empty,
            RequestHeader = entity.Header,
            RequestPayload = entity.Payload,
            ErrorMessage = entity.Message ?? string.Empty,
            ErrorStackTrace = entity.StackTrace ?? string.Empty,
            Datetime = entity.CreatedAt.UtcDateTime,
            IsResolved = false
        };

        await _queue.PushAsync(trace, ct);
    }
}
