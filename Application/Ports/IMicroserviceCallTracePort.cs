using Domain.Entities.Internals;

namespace Application.Ports;

public interface IMicroserviceCallTracePort
{
    Task<SqsSendResult> SendAsync(MicroserviceCallTraceEntity entity, CancellationToken ct = default);
}
