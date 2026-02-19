using Application.Adapters.Common;
using Domain.Entities.Client;
using Domain.Enums;
using Domain.Entities.SignatureContracts;

namespace Application.Adapters.SignatureContracts;

public class GetSignatureContractResponse
{
    public string? IdFirma { get; set; }
    public string? Referencia { get; set; }
    public string? Keyword { get; set; }
    public string? IdOrdenProveedor { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Canal { get; set; }
    public DateTime HoraExpiracion { get; set; }
    public bool FirmaEnTodosDocumentos { get; set; }
    public List<string>? IdTiposNotificacion { get; set; }
    public List<NaturalClientEntity>? Clientes { get; set; }
    public List<Documento>? Documentos { get; set; }
    public string? Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public bool Pagare { get; set; }
    public bool Vigente { get; set; }
}
