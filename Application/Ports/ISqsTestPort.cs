using Application.Adapters.Common;
using Application.Adapters.Sqs;
using Application.Internals.Executors;

namespace Application.Ports;

public interface ISqsTestPort
{
    Task<EasyResult<SqsSendTestResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        SqsSendTestRequest request,
        CancellationToken ct = default);
}
