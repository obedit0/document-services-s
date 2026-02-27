using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Executors;

namespace Application.Ports;

public interface ICancelSignatureContractPort
{
    Task<EasyResult<SignatureCancellationResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        SignatureCancellationRequest request,
        CancellationToken ct = default);
}
