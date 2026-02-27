using Domain.Entities.Internals;

namespace Application.Ports;

public interface IMicroserviceTracePort
{
    Task SaveErrorAsync(MicroserviceErrorTraceEntity entity, CancellationToken ct = default);
    Task<SqsSendResult> PublishCallAsync(MicroserviceCallTraceEntity entity, CancellationToken ct = default);
}
