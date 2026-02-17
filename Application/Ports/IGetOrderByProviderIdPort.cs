using Application.Adapters;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IGetOrderByProviderIdPort
{
    Task<EasyResult<GetOrderByProviderIdResponse>> ExecuteAsync(SignatureHeaderRequest header, GetOrderByProviderIdRequest request, CancellationToken ct = default);
}
