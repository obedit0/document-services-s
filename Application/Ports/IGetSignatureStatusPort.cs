using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IGetSignatureStatusPort
{
    Task<EasyResult<GetSignatureStatusResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        GetSignatureStatusRequest request,
        CancellationToken ct = default);
}
