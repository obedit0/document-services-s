using Application.Adapters;
using Application.Internals.Executors;

namespace Application.Ports;

public interface ISignatureContractPort
{
    Task<EasyResult<CreateSignatureContractResponse>> CreateAsync(SignatureHeaderRequest header, CreateSignatureContractRequest request, CancellationToken ct = default);

    Task<EasyResult<CancelSignatureContractResponse>> CancelAsync(SignatureHeaderRequest header, CancelSignatureContractRequest request, CancellationToken ct = default);

    Task<EasyResult<GetSignatureDocumentStatusResponse>> GetDocumentStatusAsync(GetSignatureDocumentStatusRequest request, CancellationToken ct = default);
}
