using Domain.Entities.SignatureContracts;

namespace Domain.Interfaces;

public interface IKeynuaContractClient
{
    public Task<string> CreateContractAsync(OrdenFirma orden, CancellationToken ct = default);
}
