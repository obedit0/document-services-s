using Domain.Entities.SignatureContracts;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IKeynuaContractClient
{
    public Task<string> CreateContractAsync(OrdenFirma orden, CancellationToken ct = default);
    public Task<string?> GetContractStatusAsync(string idContract, Channel canal, string messageIdentity, CancellationToken ct = default);
}
