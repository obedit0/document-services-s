using Domain.Entities.SignatureContracts;

namespace Domain.Interfaces;

public interface IKeynuaContractClient
{
    Task<KeynuaContractResult> CreateContractAsync(KeynuaContractRequest request, CancellationToken ct = default);
}
