using Domain.Entities.SignatureContracts;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IOrdenFirmaRepository
{
    Task<OrdenFirma?> GetByKeywordAsync(string keyword, CancellationToken ct = default);
    Task<OrdenFirma?> GetByProviderIdAsync(string idOrdenProveedor, CancellationToken ct = default);
    Task<string> InsertAsync(OrdenFirma entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(OrdenFirma entity, CancellationToken ct = default);

    Task<OrdenFirma?> GetByLegacyReferencesAsync(int idCanal, int idCanalTransaccion, CancellationToken ct = default);
    Task UpdateStatusAsync(string id, EstadoFirma nuevoEstado, CancellationToken ct = default);
}
