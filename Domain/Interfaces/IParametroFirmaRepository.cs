using Domain.Entities.SignatureContract;

namespace Domain.Interfaces
{
    public interface IParametroFirmaRepository
    {
        Task<ParametroFirma?> ObtenerConfiguracionAsync(int idCanal, string nombreParametro, CancellationToken ct = default);
    }
}