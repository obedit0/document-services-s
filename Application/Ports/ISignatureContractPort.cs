using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Executors;

namespace Application.Ports;

public interface ISignatureContractPort
{
    Task<EasyResult<CreateSignatureContractResponse>> CreateAsync(SignatureHeaderRequest header, CreateSignatureContractRequest request, CancellationToken ct = default);
}
