
using Application.Adapters.Examples;
using Application.Internals.Adapters;
using Application.Internals.Executors;

namespace Application.Ports;
public interface IExamplePort
{
    public Task<EasyResult<RetrieveExampleAdapter>> ShowExampleAsync(TraceIdentifierAdapter header, CancellationToken ct = default);
    public Task<EasyResult<ExecuteExampleTwoAdapter>> ExecuteExampleTwoAsync(TraceIdentifierAdapter header, CancellationToken ct = default);
}
