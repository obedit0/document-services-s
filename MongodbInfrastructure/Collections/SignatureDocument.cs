using Domain.Entities.Client;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongodbInfrastructure.Collections;

[BsonIgnoreExtraElements]
public class SignatureDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("reference")]
    public string Reference { get; set; } = string.Empty;

    [BsonElement("keyword")]
    public int Keyword { get; set; }

    [BsonElement("provider_identity")]
    public string? ProviderIdentity { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("channel")]
    public int Channel { get; set; }

    [BsonElement("expiration_time")]
    public DateTime? ExpirationTime { get; set; }

    [BsonElement("notification_type_ids")]
    public List<string>? NotificationTypeIds { get; set; }

    [BsonElement("clients")]
    public List<ClientDocument> Clients { get; set; } = new();

    [BsonElement("documents")]
    public List<DocumentDocument> Documents { get; set; } = new();

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("history")]
    public List<HistoryEventDocument> History { get; set; } = new();

    [BsonElement("promissory_note")]
    public bool PromissoryNote { get; set; }

    [BsonElement("active")]
    public bool Active { get; set; }

    public SignatureEntity ToDomain()
    {
        var channel = ParseChannel(Channel);
        var expirationTime = ExpirationTime ?? DateTime.UtcNow.AddHours(-5).AddHours(24);

        return new SignatureEntity
        {
            SignatureId = Id,
            Reference = Reference,
            Keyword = Keyword,
            ProviderIdentity = ProviderIdentity,
            Title = Title,
            Description = Description ?? string.Empty,
            Channel = channel,
            ExpirationTime = expirationTime,
            NotificationTypeIds = NotificationTypeIds ?? new List<string>(),
            PromissoryNote = PromissoryNote,
            Clients = Clients.Select(MapClientToDomain).ToList(),
            Documents = Documents.Select(d => new DocumentEntity
            {
                Name = d.Name,
                OwnerClients = d.OwnerClients ?? [],
                S3KeyOriginal = d.S3KeyOriginal,
                S3KeyFirmado = d.S3KeySigned,
                S3KeyFirmadoExpiresAt = d.S3KeySignedExpiresAt,
                ProviderKeyFirmado = d.ProviderKeySigned,
                ProviderKeyFirmadoExpiresAt = d.ProviderKeySignedExpiresAt,
                FechaFirma = d.SignedAt
            }).ToList(),
            Status = Enum.TryParse<SignatureStatus>(Status, true, out var status) ? status : SignatureStatus.PENDIENTE,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            History = History?.Select(h => new HistoryEntity
            {
                EventDate = h.EventDate,
                Source = h.Source,
                PreviousStatus = Enum.TryParse<SignatureStatus>(h.PreviousStatus, true, out var previousStatus) ? previousStatus : null,
                NewStatus = Enum.TryParse<SignatureStatus>(h.NewStatus, true, out var newStatus) ? newStatus : SignatureStatus.PENDIENTE,
                Reason = h.Reason,
                ActorId = h.ActorId,
                ProviderEventId = h.ProviderEventId
            }).ToList()
        };
    }

    public static SignatureDocument FromDomain(SignatureEntity entity)
    {
        return new SignatureDocument
        {
            Reference = entity.Reference,
            Keyword = entity.Keyword,
            ProviderIdentity = entity.ProviderIdentity,
            Title = entity.Title,
            Description = entity.Description,
            Channel = (int)entity.Channel,
            ExpirationTime = entity.ExpirationTime,
            NotificationTypeIds = entity.NotificationTypeIds,
            PromissoryNote = entity.PromissoryNote,
            Active = true,
            Clients = entity.Clients.Select(MapClientFromDomain).ToList(),
            Documents = entity.Documents.Select(d => new DocumentDocument
            {
                Name = d.Name,
                OwnerClients = d.OwnerClients ?? [],
                S3KeyOriginal = d.S3KeyOriginal,
                S3KeySigned = d.S3KeyFirmado,
                S3KeySignedExpiresAt = d.S3KeyFirmadoExpiresAt,
                ProviderKeySigned = d.ProviderKeyFirmado,
                ProviderKeySignedExpiresAt = d.ProviderKeyFirmadoExpiresAt,
                SignedAt = d.FechaFirma
            }).ToList(),
            Status = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            History = entity.History?.Select(h => new HistoryEventDocument
            {
                EventDate = h.EventDate,
                Source = h.Source,
                PreviousStatus = h.PreviousStatus?.ToString(),
                NewStatus = h.NewStatus.ToString(),
                Reason = h.Reason,
                ActorId = h.ActorId,
                ProviderEventId = h.ProviderEventId
            }).ToList()
        };
    }

    private static ClientDocument MapClientFromDomain(ClientEntity client)
    {
        var natural = client as NaturalClientEntity;
        return new ClientDocument
        {
            ClientId = client.Identity?.ToString() ?? string.Empty,
            RelationType = string.Empty,
            FullName = natural?.FullName ?? string.Empty,
            DocumentNumber = client.IdentityDocument?.Number ?? string.Empty,
            DocumentType = client.IdentityDocument?.Type?.ToString(),
            Email = client.Contact?.Email,
            Phone = client.Contact?.PhoneNumber,
            GivenName = natural?.GivenName,
            PaternalLastName = natural?.PaternalLastName,
            MaternalLastName = natural?.MaternalLastName,
            Addresses = client.Addresses?.Select(a => new AddressDocument
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

    private static NaturalClientEntity MapClientToDomain(ClientDocument client)
    {
        var identity = int.TryParse(client.ClientId, out var id) ? id : (int?)null;
        var identityDocument = BuildIdentityDocument(client);
        var contact = string.IsNullOrWhiteSpace(client.Email) && string.IsNullOrWhiteSpace(client.Phone)
            ? null
            : new ContactEntity { Email = client.Email, PhoneNumber = client.Phone };
        var addresses = client.Addresses?.Select(a => new AddressEntity
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
            FullName = client.FullName,
            GivenName = client.GivenName,
            PaternalLastName = client.PaternalLastName,
            MaternalLastName = client.MaternalLastName,
            IdentityDocument = identityDocument,
            Contact = contact,
            Addresses = addresses
        };
    }

    private static IdentityDocumentEntity? BuildIdentityDocument(ClientDocument client)
    {
        if (string.IsNullOrWhiteSpace(client.DocumentNumber) && string.IsNullOrWhiteSpace(client.DocumentType))
            return null;

        var identityDocument = new IdentityDocumentEntity
        {
            Number = client.DocumentNumber
        };

        if (!string.IsNullOrWhiteSpace(client.DocumentType) &&
            Enum.TryParse<DocumentType>(client.DocumentType, true, out var type))
        {
            identityDocument.Type = type;
        }

        return identityDocument;
    }

    private static ChannelEnum ParseChannel(int value)
    {
        return Enum.IsDefined(typeof(ChannelEnum), value)
            ? (ChannelEnum)value
            : ChannelEnum.Ventanilla;
    }
}

[BsonIgnoreExtraElements]
public class ClientDocument
{
    [BsonElement("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [BsonElement("relation_type")]
    public string RelationType { get; set; } = string.Empty;

    [BsonElement("full_name")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("document_number")]
    public string DocumentNumber { get; set; } = string.Empty;

    [BsonElement("document_type")]
    public string? DocumentType { get; set; }

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

    [BsonElement("phone")]
    public string? Phone { get; set; }
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
public class DocumentDocument
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("owner_clients")]
    public List<string> OwnerClients { get; set; } = [];

    [BsonElement("s3_key_original")]
    public string S3KeyOriginal { get; set; } = string.Empty;

    [BsonElement("s3_key_signed")]
    public string? S3KeySigned { get; set; }

    [BsonElement("s3_key_signed_expires_at")]
    public DateTime? S3KeySignedExpiresAt { get; set; }

    [BsonElement("provider_key_signed")]
    public string? ProviderKeySigned { get; set; }

    [BsonElement("provider_key_signed_expires_at")]
    public DateTime? ProviderKeySignedExpiresAt { get; set; }

    [BsonElement("signed_at")]
    public DateTime? SignedAt { get; set; }
}

[BsonIgnoreExtraElements]
public class HistoryEventDocument
{
    [BsonElement("event_date")]
    public DateTime EventDate { get; set; }

    [BsonElement("source")]
    public string? Source { get; set; }

    [BsonElement("previous_status")]
    public string? PreviousStatus { get; set; }

    [BsonElement("new_status")]
    public string NewStatus { get; set; } = string.Empty;

    [BsonElement("reason")]
    public string? Reason { get; set; }

    [BsonElement("actor_id")]
    public string? ActorId { get; set; }

    [BsonElement("provider_event_id")]
    public string? ProviderEventId { get; set; }
}
