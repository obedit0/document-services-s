using Application.Adapters.Healthcheck;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IHealthcheckPort
{
    Task<EasyResult<HealthcheckDependenciesResponse>> ExecuteAsync(CancellationToken ct = default);
}
