using Application.Adapters.Common;
using Application.Adapters.SignatureContracts.DocumentSignatureCompletion;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IUpdateProviderDocumentsPort
{
    Task<EasyResult<SignatureCompletionResponse>> ExecuteAsync(SignatureHeaderRequest header, SignatureCompletionRequest request, CancellationToken ct = default);
}
