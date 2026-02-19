using Application.Ports;
using Domain.Entities.Internals;
using Domain.Interfaces;

namespace Application.Usecases.MicroserviceCallTraceUsecase;

public sealed class MicroserviceCallTraceSqsCase : IMicroserviceCallTracePort
{
    private readonly IMicroserviceCallTracePublisher _publisher;

    public MicroserviceCallTraceSqsCase(IMicroserviceCallTracePublisher publisher)
    {
        _publisher = publisher;
    }

    public Task<SqsSendResult> SendAsync(MicroserviceCallTraceEntity entity, CancellationToken ct = default)
    {
        return _publisher.SendAsync(entity, ct);
    }
}
