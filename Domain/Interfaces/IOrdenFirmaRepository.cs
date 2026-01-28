using Domain.Entities.SignatureContracts;

namespace Domain.Interfaces;

public interface IOrdenFirmaRepository
{
    Task<OrdenFirma?> GetByReferenciaAsync(string referencia, CancellationToken ct = default);
    Task InsertAsync(OrdenFirma entity, CancellationToken ct = default);
}
