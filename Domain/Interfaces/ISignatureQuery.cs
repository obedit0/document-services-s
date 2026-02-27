using Domain.Entities.SignatureContracts;

namespace Domain.Interfaces;

public interface ISignatureQuery
{
    Task<SignatureEntity?> GetByKeywordAndChannelAsync(int keyword, int channel, CancellationToken ct = default);
    Task<int> CountByKeywordAndChannelAsync(int keyword, int channel, CancellationToken ct = default);
}
