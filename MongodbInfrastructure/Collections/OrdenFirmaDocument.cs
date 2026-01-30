using Domain.Entities.Client;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongodbInfrastructure.Collections;

[BsonIgnoreExtraElements]
public class OrdenFirmaDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("referencia")]
    public string Referencia { get; set; } = string.Empty;

    [BsonElement("keyword")]
    public string Keyword { get; set; } = string.Empty;

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
    public DateTime? HoraExpiracion { get; set; }

    [BsonElement("firma_en_todos_documentos")]
    public bool? FirmaEnTodosDocumentos { get; set; }

    [BsonElement("id_tipos_notificacion")]
    public List<string>? IdTiposNotificacion { get; set; }

    [BsonElement("clientes")]
    public List<ClienteDocument> Clientes { get; set; } = new();

    [BsonElement("documentos")]
    public List<DocumentoDocument> Documentos { get; set; } = new();

    [BsonElement("estado")]
    public string Estado { get; set; } = string.Empty;

    [BsonElement("fecha_creacion")]
    public DateTimeOffset FechaCreacion { get; set; }

    [BsonElement("fecha_actualizacion")]
    public DateTimeOffset FechaActualizacion { get; set; }

    [BsonElement("historico")]
    public List<HistoricoEventoDocument>? Historico { get; set; }

    [BsonElement("pagare")]
    public bool Pagare { get; set; }

    public OrdenFirma ToDomain()
    {
        var canal = ParseChannel(Canal);
        var horaExpiracion = HoraExpiracion ?? DateTime.UtcNow.AddHours(24);
        var keyword = string.IsNullOrWhiteSpace(Keyword) ? Referencia : Keyword;

        return new OrdenFirma
        {
            IdFirma = Id,
            Referencia = Referencia,
            Keyword = keyword,
            IdOrdenProveedor = IdOrdenProveedor,
            Titulo = Titulo,
            Descripcion = Descripcion ?? string.Empty,
            Canal = canal,
            HoraExpiracion = horaExpiracion,
            FirmaEnTodosDocumentos = FirmaEnTodosDocumentos ?? false,
            IdTiposNotificacion = IdTiposNotificacion ?? new List<string>(),
            Pagare = Pagare,
            Clientes = Clientes.Select(c => new NaturalClientEntity
            {
                Identity = int.TryParse(c.IdCliente, out var id) ? id : (int?)null,
                FullName = c.NombreCompleto,
                IdentityDocument = string.IsNullOrWhiteSpace(c.NumeroDocumento)
                    ? null
                    : new IdentityDocumentEntity { Number = c.NumeroDocumento },
                Contact = string.IsNullOrWhiteSpace(c.Email) && string.IsNullOrWhiteSpace(c.Telefono)
                    ? null
                    : new ContactEntity { Email = c.Email, PhoneNumber = c.Telefono }
            }).Cast<ClientEntity>().ToList(),
            Documentos = Documentos.Select(d => new Documento
            {
                IdDocumento = d.IdDocumento,
                TipoDocumento = d.TipoDocumento,
                NombreDocumento = string.IsNullOrWhiteSpace(d.NombreDocumento) ? d.IdDocumento : d.NombreDocumento,
                OwnerClient = d.OwnerClienteId,
                S3KeyOriginal = d.S3KeyOriginal,
                HashSha256 = d.HashSha256,
                S3KeyFirmado = d.S3KeyFirmado,
                ProviderKeyFirmado = d.ProviderKeyFirmado,
                FechaFirma = d.FechaFirma
            }).ToList(),
            Estado = Enum.TryParse<EstadoFirma>(Estado, true, out var estado) ? estado : EstadoFirma.PENDIENTE,
            FechaCreacion = FechaCreacion,
            FechaActualizacion = FechaActualizacion,
            Historico = Historico?.Select(h => new HistoricoEvento
            {
                FechaEvento = h.FechaEvento.UtcDateTime,
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
            Referencia = entity.Referencia,
            Keyword = entity.Keyword,
            IdOrdenProveedor = entity.IdOrdenProveedor,
            Titulo = entity.Titulo,
            Descripcion = entity.Descripcion,
            Canal = entity.Canal.ToString(),
            HoraExpiracion = entity.HoraExpiracion,
            FirmaEnTodosDocumentos = entity.FirmaEnTodosDocumentos,
            IdTiposNotificacion = entity.IdTiposNotificacion,
            Pagare = entity.Pagare,
            Clientes = entity.Clientes.Select(c =>
            {
                var nombreCompleto = c is NaturalClientEntity natural ? natural.FullName : null;
                return new ClienteDocument
                {
                    IdCliente = c.Identity?.ToString() ?? string.Empty,
                    TipoVinculo = string.Empty,
                    NombreCompleto = nombreCompleto ?? string.Empty,
                    NumeroDocumento = c.IdentityDocument?.Number ?? string.Empty,
                    Email = c.Contact?.Email,
                    Telefono = c.Contact?.PhoneNumber
                };
            }).ToList(),
            Documentos = entity.Documentos.Select(d => new DocumentoDocument
            {
                IdDocumento = d.IdDocumento,
                TipoDocumento = d.TipoDocumento,
                NombreDocumento = d.NombreDocumento,
                OwnerClienteId = d.OwnerClient,
                S3KeyOriginal = d.S3KeyOriginal,
                HashSha256 = d.HashSha256,
                S3KeyFirmado = d.S3KeyFirmado,
                ProviderKeyFirmado = d.ProviderKeyFirmado,
                FechaFirma = d.FechaFirma
            }).ToList(),
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

    private static Channel ParseChannel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Channel.Ventanilla;

        if (int.TryParse(value, out var numeric) && Enum.IsDefined(typeof(Channel), numeric))
            return (Channel)numeric;

        return Enum.TryParse<Channel>(value, true, out var channel)
            ? channel
            : Channel.Ventanilla;
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
    public string NumeroDocumento { get; set; } = string.Empty;


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

    [BsonElement("nombre_documento")]
    public string NombreDocumento { get; set; } = string.Empty;

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
