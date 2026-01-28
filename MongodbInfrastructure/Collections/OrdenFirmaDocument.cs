using Domain.Entities.SignatureContracts;
using MongoDB.Bson.Serialization.Attributes;

namespace MongodbInfrastructure.Collections;

[BsonIgnoreExtraElements]
public class OrdenFirmaDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    [BsonElement("referencia")]
    public string Referencia { get; set; } = string.Empty;

    [BsonElement("proveedor")]
    public string Proveedor { get; set; } = string.Empty;

    [BsonElement("id_orden_proveedor")]
    public string? IdOrdenProveedor { get; set; }

    [BsonElement("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [BsonElement("descripcion")]
    public string? Descripcion { get; set; }

    [BsonElement("canal")]
    public string? Canal { get; set; }

    [BsonElement("hora_expiracion")]
    public DateTimeOffset? HoraExpiracion { get; set; }

    [BsonElement("firma_en_todos_documentos")]
    public bool? FirmaEnTodosDocumentos { get; set; }

    [BsonElement("id_tipos_notificacion")]
    public List<string>? IdTiposNotificacion { get; set; }

    [BsonElement("clientes")]
    public List<ClienteDocument> Clientes { get; set; } = new();

    [BsonElement("documentos")]
    public List<DocumentoDocument> Documentos { get; set; } = new();

    [BsonElement("observadores")]
    public List<ObservadorDocument>? Observadores { get; set; }

    [BsonElement("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = string.Empty;

    [BsonElement("fecha_creacion")]
    public DateTimeOffset FechaCreacion { get; set; }

    [BsonElement("fecha_actualizacion")]
    public DateTimeOffset FechaActualizacion { get; set; }

    [BsonElement("historico")]
    public List<HistoricoEventoDocument>? Historico { get; set; }

    public OrdenFirma ToDomain()
    {
        return new OrdenFirma
        {
            Id = Id,
            Referencia = new ReferenciaFirma(Referencia),
            Proveedor = Proveedor,
            IdOrdenProveedor = IdOrdenProveedor,
            Titulo = Titulo,
            Descripcion = Descripcion,
            Canal = Canal,
            HoraExpiracion = HoraExpiracion,
            FirmaEnTodosDocumentos = FirmaEnTodosDocumentos,
            IdTiposNotificacion = IdTiposNotificacion,
            Clientes = Clientes.Select(c => new Cliente
            {
                IdCliente = c.IdCliente,
                TipoVinculo = c.TipoVinculo,
                NombreCompleto = c.NombreCompleto,
                Email = c.Email,
                Telefono = c.Telefono
            }).ToList(),
            Documentos = Documentos.Select(d => new Documento
            {
                IdDocumento = d.IdDocumento,
                TipoDocumento = d.TipoDocumento,
                OwnerClienteId = d.OwnerClienteId,
                S3KeyOriginal = d.S3KeyOriginal,
                HashSha256 = d.HashSha256,
                S3KeyFirmado = d.S3KeyFirmado,
                ProviderKeyFirmado = d.ProviderKeyFirmado,
                FechaFirma = d.FechaFirma
            }).ToList(),
            Observadores = Observadores?.Select(o => new Observador
            {
                IdObservador = o.IdObservador,
                Email = o.Email,
                Rol = o.Rol
            }).ToList(),
            Metadata = Metadata,
            Estado = Enum.TryParse<EstadoFirma>(Estado, true, out var estado) ? estado : EstadoFirma.PENDIENTE,
            FechaCreacion = FechaCreacion,
            FechaActualizacion = FechaActualizacion,
            Historico = Historico?.Select(h => new HistoricoEvento
            {
                FechaEvento = h.FechaEvento,
                Fuente = h.Fuente,
                EstadoAnterior = Enum.TryParse<EstadoFirma>(h.EstadoAnterior, true, out var estadoAnterior) ? estadoAnterior : null,
                EstadoNuevo = Enum.TryParse<EstadoFirma>(h.EstadoNuevo, true, out var estadoNuevo) ? estadoNuevo : EstadoFirma.PENDIENTE,
                Motivo = h.Motivo,
                ActorId = h.ActorId,
                ProviderEventId = h.ProviderEventId
            }).ToList()
        };
    }

    public static OrdenFirmaDocument FromDomain(OrdenFirma entity)
    {
        return new OrdenFirmaDocument
        {
            Id = entity.Id,
            Referencia = entity.Referencia.Value,
            Proveedor = entity.Proveedor,
            IdOrdenProveedor = entity.IdOrdenProveedor,
            Titulo = entity.Titulo,
            Descripcion = entity.Descripcion,
            Canal = entity.Canal,
            HoraExpiracion = entity.HoraExpiracion,
            FirmaEnTodosDocumentos = entity.FirmaEnTodosDocumentos,
            IdTiposNotificacion = entity.IdTiposNotificacion,
            Clientes = entity.Clientes.Select(c => new ClienteDocument
            {
                IdCliente = c.IdCliente,
                TipoVinculo = c.TipoVinculo,
                NombreCompleto = c.NombreCompleto,
                Email = c.Email,
                Telefono = c.Telefono
            }).ToList(),
            Documentos = entity.Documentos.Select(d => new DocumentoDocument
            {
                IdDocumento = d.IdDocumento,
                TipoDocumento = d.TipoDocumento,
                OwnerClienteId = d.OwnerClienteId,
                S3KeyOriginal = d.S3KeyOriginal,
                HashSha256 = d.HashSha256,
                S3KeyFirmado = d.S3KeyFirmado,
                ProviderKeyFirmado = d.ProviderKeyFirmado,
                FechaFirma = d.FechaFirma
            }).ToList(),
            Observadores = entity.Observadores?.Select(o => new ObservadorDocument
            {
                IdObservador = o.IdObservador,
                Email = o.Email,
                Rol = o.Rol
            }).ToList(),
            Metadata = entity.Metadata,
            Estado = entity.Estado.ToString(),
            FechaCreacion = entity.FechaCreacion,
            FechaActualizacion = entity.FechaActualizacion,
            Historico = entity.Historico?.Select(h => new HistoricoEventoDocument
            {
                FechaEvento = h.FechaEvento,
                Fuente = h.Fuente,
                EstadoAnterior = h.EstadoAnterior?.ToString(),
                EstadoNuevo = h.EstadoNuevo.ToString(),
                Motivo = h.Motivo,
                ActorId = h.ActorId,
                ProviderEventId = h.ProviderEventId
            }).ToList()
        };
    }
}

public class ClienteDocument
{
    [BsonElement("id_cliente")]
    public string IdCliente { get; set; } = string.Empty;

    [BsonElement("tipo_vinculo")]
    public string TipoVinculo { get; set; } = string.Empty;

    [BsonElement("nombre_completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("telefono")]
    public string? Telefono { get; set; }
}

public class DocumentoDocument
{
    [BsonElement("id_documento")]
    public string IdDocumento { get; set; } = string.Empty;

    [BsonElement("tipo_documento")]
    public string TipoDocumento { get; set; } = string.Empty;

    [BsonElement("owner_cliente_id")]
    public string OwnerClienteId { get; set; } = string.Empty;

    [BsonElement("s3_key_original")]
    public string S3KeyOriginal { get; set; } = string.Empty;

    [BsonElement("hash_sha256")]
    public string? HashSha256 { get; set; }

    [BsonElement("s3_key_firmado")]
    public string? S3KeyFirmado { get; set; }

    [BsonElement("provider_key_firmado")]
    public string? ProviderKeyFirmado { get; set; }

    [BsonElement("fecha_firma")]
    public DateTimeOffset? FechaFirma { get; set; }
}

public class ObservadorDocument
{
    [BsonElement("id_observador")]
    public string IdObservador { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("rol")]
    public string? Rol { get; set; }
}

public class HistoricoEventoDocument
{
    [BsonElement("fecha_evento")]
    public DateTimeOffset FechaEvento { get; set; }

    [BsonElement("fuente")]
    public string? Fuente { get; set; }

    [BsonElement("estado_anterior")]
    public string? EstadoAnterior { get; set; }

    [BsonElement("estado_nuevo")]
    public string EstadoNuevo { get; set; } = string.Empty;

    [BsonElement("motivo")]
    public string? Motivo { get; set; }

    [BsonElement("actor_id")]
    public string? ActorId { get; set; }

    [BsonElement("provider_event_id")]
    public string? ProviderEventId { get; set; }
}
