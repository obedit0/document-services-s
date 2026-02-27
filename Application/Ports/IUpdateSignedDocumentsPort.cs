using Application.Adapters.Common;
using Application.Adapters.SignatureContracts.UpdateDocuments;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IUpdateSignedDocumentsPort
{
    Task<EasyResult<SignatureUpdateDocumentResponse>> ExecuteAsync(SignatureHeaderRequest header, SignatureUpdateDocumentRequest request, CancellationToken ct = default);
}
