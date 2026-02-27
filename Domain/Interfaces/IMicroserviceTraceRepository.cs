using Domain.Entities.Internals;

namespace Domain.Interfaces;

public interface IMicroserviceTraceRepository
{
    Task InsertErrorAsync(MicroserviceErrorTraceEntity entity, CancellationToken ct = default);
    Task<SqsSendResult> PublishCallAsync(MicroserviceCallTraceEntity entity, CancellationToken ct = default);
}
