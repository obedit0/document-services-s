using Domain.Entities.SignatureContracts;

namespace Domain.Interfaces;

public interface IOrdenFirmaRepository
{
    Task<OrdenFirma?> GetByKeywordAsync(string keyword, CancellationToken ct = default);
    Task<string> InsertAsync(OrdenFirma entity, CancellationToken ct = default);
}
