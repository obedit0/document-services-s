using Domain.Entities.Client;
using Domain.Entities.SignatureContract;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongodbInfrastructure.Serializers;
using System.Reflection.Metadata;

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

    [BsonElement("id_orden_proveedor")]
    public string? IdOrdenProveedor { get; set; }

    [BsonElement("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [BsonElement("descripcion")]
    public string? Descripcion { get; set; }

    [BsonElement("canal")]
    [BsonSerializer(typeof(ChannelSerializer))]
    public int Canal { get; set; }


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
    public DateTime FechaCreacion { get; set; }

    [BsonElement("fecha_actualizacion")]
    public DateTime FechaActualizacion { get; set; }

    [BsonElement("historico")]
    public List<HistoricoEventoDocument>? Historico { get; set; }

    [BsonElement("pagare")]
    public bool Pagare { get; set; }

    [BsonElement("vigente")]
    public bool Vigente { get; set; }

    [BsonElement("vigenciaKeynua")]
    public DateTime? VigenciaKeynua { get; set; }

    [BsonElement("vigenciaS3")]
    public DateTime? VigenciaS3 { get; set; }

    [BsonElement("documentosFirmados")]
    public List<DocumentoFirmadoDocument>? DocumentosFirmados { get; set; }

    public OrdenFirma ToDomain()
    {
        var canal = ParseChannel(Canal);
        var horaExpiracion = HoraExpiracion ?? DateTime.UtcNow.AddHours(-5).AddHours(24);
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
            Clientes = Clientes.Select(MapClienteToDomain).ToList(),
            Documentos = Documentos.Select(d => new Documento
            {
                Name = d.Name,
                OwnerClients = d.OwnerClientes ?? [],
                S3KeyOriginal = d.S3KeyOriginal,
                S3KeyFirmado = d.S3KeyFirmado,
                S3KeyFirmadoExpiresAt = d.S3KeyFirmadoExpiresAt,
                ProviderKeyFirmado = d.ProviderKeyFirmado,
                ProviderKeyFirmadoExpiresAt = d.ProviderKeyFirmadoExpiresAt,
                FechaFirma = d.FechaFirma
            }).ToList(),
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
            }).ToList(),
            VigenciaKeynua = VigenciaKeynua,
            VigenciaS3 = VigenciaS3,
            DocumentosFirmados = DocumentosFirmados?.Select(d => new DocumentoFirmado
            {
                Nombre = d.Nombre,
                Url = d.Url,
                Tipo = d.Tipo
            }).ToList() ?? new List<DocumentoFirmado>()
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
            Canal = (int)entity.Canal,
            HoraExpiracion = entity.HoraExpiracion,
            FirmaEnTodosDocumentos = entity.FirmaEnTodosDocumentos,
            IdTiposNotificacion = entity.IdTiposNotificacion,
            Pagare = entity.Pagare,
            Vigente = true,
            Clientes = entity.Clientes.Select(MapClienteFromDomain).ToList(),
            Documentos = entity.Documentos.Select(d => new DocumentoDocument
            {
                Name = d.Name,
                OwnerClientes = d.OwnerClients ?? [],
                S3KeyOriginal = d.S3KeyOriginal,
                S3KeyFirmado = d.S3KeyFirmado,
                S3KeyFirmadoExpiresAt = d.S3KeyFirmadoExpiresAt,
                ProviderKeyFirmado = d.ProviderKeyFirmado,
                ProviderKeyFirmadoExpiresAt = d.ProviderKeyFirmadoExpiresAt,
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
            }).ToList(),
            VigenciaKeynua = entity.VigenciaKeynua,
            VigenciaS3 = entity.VigenciaS3,
            DocumentosFirmados = entity.DocumentosFirmados?.Select(d => new DocumentoFirmadoDocument
            {
                Nombre = d.Nombre,
                Url = d.Url,
                Tipo = d.Tipo
            }).ToList()
        };
    }

    private static ClienteDocument MapClienteFromDomain(ClientEntity cliente)
    {
        var natural = cliente as NaturalClientEntity;
        return new ClienteDocument
        {
            IdCliente = cliente.Identity?.ToString() ?? string.Empty,
            TipoVinculo = string.Empty,
            NombreCompleto = natural?.FullName ?? string.Empty,
            NumeroDocumento = cliente.IdentityDocument?.Number ?? string.Empty,
            TipoDocumento = cliente.IdentityDocument?.Type?.ToString(),
            Email = cliente.Contact?.Email,
            Telefono = cliente.Contact?.PhoneNumber,
            GivenName = natural?.GivenName,
            PaternalLastName = natural?.PaternalLastName,
            MaternalLastName = natural?.MaternalLastName,
            Addresses = cliente.Addresses?.Select(a => new AddressDocument
            {
                Identity = a.Identity,
                Name = a.Name,
                Street = a.Street,
                Number = a.Number,
                Reference = a.Reference,
                PostalCode = a.PostalCode
            }).ToList()
        };
    }

    private static NaturalClientEntity MapClienteToDomain(ClienteDocument cliente)
    {
        var identity = int.TryParse(cliente.IdCliente, out var id) ? id : (int?)null;
        var identityDocument = BuildIdentityDocument(cliente);
        var contact = string.IsNullOrWhiteSpace(cliente.Email) && string.IsNullOrWhiteSpace(cliente.Telefono)
            ? null
            : new ContactEntity { Email = cliente.Email, PhoneNumber = cliente.Telefono };
        var addresses = cliente.Addresses?.Select(a => new AddressEntity
        {
            Identity = a.Identity,
            Name = a.Name,
            Street = a.Street,
            Number = a.Number,
            Reference = a.Reference,
            PostalCode = a.PostalCode
        }).ToList();

        return new NaturalClientEntity
        {
            Identity = identity,
            FullName = cliente.NombreCompleto,
            GivenName = cliente.GivenName,
            PaternalLastName = cliente.PaternalLastName,
            MaternalLastName = cliente.MaternalLastName,
            IdentityDocument = identityDocument,
            Contact = contact,
            Addresses = addresses
        };
    }

    private static IdentityDocumentEntity? BuildIdentityDocument(ClienteDocument cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.NumeroDocumento) && string.IsNullOrWhiteSpace(cliente.TipoDocumento))
            return null;

        var identityDocument = new IdentityDocumentEntity
        {
            Number = cliente.NumeroDocumento
        };

        if (!string.IsNullOrWhiteSpace(cliente.TipoDocumento) &&
            Enum.TryParse<DocumentType>(cliente.TipoDocumento, true, out var type))
        {
            identityDocument.Type = type;
        }

        return identityDocument;
    }

    private static Channel ParseChannel(int value)
    {
        return Enum.IsDefined(typeof(Channel), value)
            ? (Channel)value
            : Channel.Ventanilla;
    }
}

[BsonIgnoreExtraElements]
public class ClienteDocument
{
    [BsonElement("id_cliente")]
    public string IdCliente { get; set; } = string.Empty;

    [BsonElement("tipo_vinculo")]
    public string TipoVinculo { get; set; } = string.Empty;

    [BsonElement("nombre_completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [BsonElement("numero_documento")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [BsonElement("tipo_documento")]
    public string? TipoDocumento { get; set; }

    [BsonElement("given_name")]
    public string? GivenName { get; set; }

    [BsonElement("paternal_last_name")]
    public string? PaternalLastName { get; set; }

    [BsonElement("maternal_last_name")]
    public string? MaternalLastName { get; set; }

    [BsonElement("addresses")]
    public List<AddressDocument>? Addresses { get; set; }


    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("telefono")]
    public string? Telefono { get; set; }
}

[BsonIgnoreExtraElements]
public class AddressDocument
{
    [BsonElement("identity")]
    public int? Identity { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("street")]
    public string? Street { get; set; }

    [BsonElement("number")]
    public string? Number { get; set; }

    [BsonElement("reference")]
    public string? Reference { get; set; }

    [BsonElement("postal_code")]
    public string? PostalCode { get; set; }
}


[BsonIgnoreExtraElements]
public class DocumentoDocument
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("owner_clientes")]
    public List<string> OwnerClientes { get; set; } = [];

    [BsonElement("s3_key_original")]
    public string S3KeyOriginal { get; set; } = string.Empty;

    [BsonElement("s3_key_firmado")]
    public string? S3KeyFirmado { get; set; }

    [BsonElement("s3_key_firmado_expires_at")]
    public DateTime? S3KeyFirmadoExpiresAt { get; set; }

    [BsonElement("provider_key_firmado")]
    public string? ProviderKeyFirmado { get; set; }

    [BsonElement("provider_key_firmado_expires_at")]
    public DateTime? ProviderKeyFirmadoExpiresAt { get; set; }

    [BsonElement("fecha_firma")]
    public DateTime? FechaFirma { get; set; }
}


[BsonIgnoreExtraElements]
public class HistoricoEventoDocument
{
    [BsonElement("fecha_evento")]
    public DateTime FechaEvento { get; set; }

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
