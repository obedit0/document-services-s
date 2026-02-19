using Application.Adapters.Common;
using Application.Adapters.UpdateDocuments;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IUpdateProviderDocumentsPort
{
    Task<EasyResult<UpdateProviderDocumentsResponse>> ExecuteAsync(SignatureHeaderRequest header, UpdateProviderDocumentsRequest request, CancellationToken ct = default);
}
