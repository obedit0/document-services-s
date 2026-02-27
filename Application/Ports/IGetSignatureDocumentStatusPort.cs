using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IGetSignatureDocumentStatusPort
{
    Task<EasyResult<GetSignatureDocumentStatusResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        GetSignatureDocumentStatusRequest request,
        CancellationToken ct = default);
}
