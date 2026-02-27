using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Executors;

namespace Application.Ports;

public interface IGetSignatureStatusPort
{
    Task<EasyResult<SignatureInquiryResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        SignatureInquiryRequest request,
        CancellationToken ct = default);
}
