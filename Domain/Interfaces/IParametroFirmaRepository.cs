using Domain.Entities.SignatureContracts;

namespace Domain.Interfaces
{
    public interface IParametroFirmaRepository
    {
        Task<ParametroFirma?> ObtenerConfiguracionAsync(int idCanal, string nombreParametro, CancellationToken ct = default);
    }
}