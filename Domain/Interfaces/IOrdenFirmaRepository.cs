using Domain.Entities.SignatureContracts;

namespace Domain.Interfaces;


public interface IOrdenFirmaRepository
{
    Task<OrdenFirma?> GetByKeywordAsync(string keyword, CancellationToken ct = default);
    Task<OrdenFirma?> GetByProviderIdAsync(string idOrdenProveedor, CancellationToken ct = default);
    Task<string> InsertAsync(OrdenFirma entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(OrdenFirma entity, CancellationToken ct = default);
    Task<OrdenFirma?> GetByKeywordAndChannelAsync(string keyword, int channel, CancellationToken ct = default);
    Task<int> GetCountByKeywordAndChannelAsync(string keyword, int channel, CancellationToken ct = default);
}
