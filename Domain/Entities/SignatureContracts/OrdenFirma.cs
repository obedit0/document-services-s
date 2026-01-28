namespace Domain.Entities.SignatureContracts;

public class OrdenFirma
{
    public required string Id { get; set; }
    public required ReferenciaFirma Referencia { get; set; }
    public required string Proveedor { get; set; }
    public string? IdOrdenProveedor { get; set; }
    public required string Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Canal { get; set; }
    public DateTimeOffset? HoraExpiracion { get; set; }
    public bool? FirmaEnTodosDocumentos { get; set; }
    public List<string>? IdTiposNotificacion { get; set; }
    public required List<Cliente> Clientes { get; set; }
    public required List<Documento> Documentos { get; set; }
    public List<Observador>? Observadores { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public EstadoFirma Estado { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public DateTimeOffset FechaActualizacion { get; set; }
    public List<HistoricoEvento>? Historico { get; set; }
}
