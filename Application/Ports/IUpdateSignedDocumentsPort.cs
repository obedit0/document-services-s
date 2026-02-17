using Application.Adapters;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IUpdateSignedDocumentsPort
{
    Task<EasyResult<UpdateSignedDocumentsResponse>> ExecuteAsync(SignatureHeaderRequest header, UpdateSignedDocumentsRequest request, CancellationToken ct = default);
}
