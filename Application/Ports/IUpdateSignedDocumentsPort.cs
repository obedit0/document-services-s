using Application.Adapters.Common;
using Application.Adapters.UpdateDocuments;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IUpdateSignedDocumentsPort
{
    Task<EasyResult<UpdateSignedDocumentsResponse>> ExecuteAsync(SignatureHeaderRequest header, UpdateSignedDocumentsRequest request, CancellationToken ct = default);
}
