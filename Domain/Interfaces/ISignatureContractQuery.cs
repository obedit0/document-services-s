using Domain.Enums;

namespace Domain.Interfaces;

public interface ISignatureContractQuery
{
    Task<SignatureStatus> GetStatusAsync(string contractId, ChannelEnum channel, string messageIdentity, CancellationToken ct = default);
}
