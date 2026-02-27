using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Executors;

namespace Application.Ports;

public interface ISignatureContractPort
{
    Task<EasyResult<SignatureResponse>> CreateAsync(SignatureHeaderRequest header, SignatureRequest request, CancellationToken ct = default);
}
