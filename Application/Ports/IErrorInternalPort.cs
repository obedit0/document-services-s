using Domain.Entities.Internals;

namespace Application.Ports;

public interface IErrorInternalPort
{
    Task SaveAsync(MicroserviceErrorEntity entity, CancellationToken ct = default);
}
