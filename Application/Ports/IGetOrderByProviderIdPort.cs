using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IGetOrderByProviderIdPort
{
    Task<EasyResult<GetOrderByProviderIdResponse>> ExecuteAsync(SignatureHeaderRequest header, GetOrderByProviderIdRequest request, CancellationToken ct = default);
}
