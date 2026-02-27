using Domain.Entities.SignatureContracts;

namespace Domain.Interfaces;

public interface ISignatureContractCommand
{
    Task<string> CreateAsync(SignatureEntity orden, CancellationToken ct = default);
    Task CancelAsync(string contractId, CancellationToken ct = default);
}
