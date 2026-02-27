using Application.Ports;
using Domain.Entities.Internals;
using Domain.Interfaces;

namespace Application.Usecases.ErrorTrace;

public sealed class MicroserviceTracePersistenceCase : IMicroserviceTracePort
{
    private readonly IMicroserviceTraceRepository _repository;

    public MicroserviceTracePersistenceCase(IMicroserviceTraceRepository repository)
    {
        _repository = repository;
    }

    public Task SaveErrorAsync(MicroserviceErrorTraceEntity entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return _repository.InsertErrorAsync(entity, ct);
    }

    public Task<SqsSendResult> PublishCallAsync(MicroserviceCallTraceEntity entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return _repository.PublishCallAsync(entity, ct);
    }
}
