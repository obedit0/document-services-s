using Domain.Entities.Client;
using Domain.Enums;

namespace Domain.Entities.SignatureContracts;

public class OrdenFirma
{
    public string IdFirma { get; set; }
    public required string Referencia { get; set; }
    public required string Keyword { get; set; }
    public string? IdOrdenProveedor { get; set; }
    public required string Titulo { get; set; }
    public required string Descripcion { get; set; }
    public required Channel Canal { get; set; }
    public required DateTime HoraExpiracion { get; set; }
    public bool FirmaEnTodosDocumentos { get; set; }
    public required List<string> IdTiposNotificacion { get; set; }
    public required List<NaturalClientEntity> Clientes { get; set; }
    public required List<Documento> Documentos { get; set; }
    public EstadoFirma Estado { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public DateTimeOffset FechaActualizacion { get; set; }
    public List<HistoricoEvento>? Historico { get; set; }
    public required bool Pagare { get; set; } 
}
