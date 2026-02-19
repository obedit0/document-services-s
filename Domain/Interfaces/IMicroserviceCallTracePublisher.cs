using Domain.Entities.Internals;

namespace Domain.Interfaces;

public interface IMicroserviceCallTracePublisher
{
    Task<SqsSendResult> SendAsync(MicroserviceCallTraceEntity entity, CancellationToken ct = default);
}
